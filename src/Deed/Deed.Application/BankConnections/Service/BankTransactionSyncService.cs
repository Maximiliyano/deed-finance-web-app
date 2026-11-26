using System.Globalization;
using System.Net;
using Deed.Application.Abstractions.Services;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.BankNotifications;
using Deed.Application.BankNotifications.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using Serilog;

namespace Deed.Application.BankConnections.Service;

public sealed class BankTransactionSyncService(
    IBankConnectionRepository connectionRepository,
    IBankNotificationRepository notificationRepository,
    IUserSettingsRepository userSettingsRepository,
    IPrivatAutoclientService bank,
    ITokenProtector tokenProtector,
    IEmailService emailService,
    IBankNotificationPublisher publisher,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IBankTransactionSyncService
{
    private static readonly TimeSpan Lookback = TimeSpan.FromHours(12);
    private static readonly TimeSpan BaselineTolerance = TimeSpan.FromMinutes(10);

    public Task<int> SyncAsync(CancellationToken cancellationToken)
        => SyncAsync(new ActiveBankConnectionsSpecification(), cancellationToken);

    public Task<int> SyncUserAsync(string createdBy, CancellationToken cancellationToken)
        => SyncAsync(new ActiveBankConnectionsSpecification(createdBy), cancellationToken);

    private async Task<int> SyncAsync(ActiveBankConnectionsSpecification specification, CancellationToken cancellationToken)
    {
        var connections = (await connectionRepository
            .GetAllAsync(specification, cancellationToken)
            .ConfigureAwait(false)).ToList();

        var created = 0;

        foreach (var connection in connections)
        {
            try
            {
                created += await SyncConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(ex, "Failed to sync bank transactions for user {User}", connection.CreatedBy);
            }
        }

        return created;
    }

    private async Task<int> SyncConnectionAsync(BankConnection connection, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var secret = tokenProtector.Unprotect(connection.EncryptedToken);

        if (secret is null)
        {
            await MarkTokenInvalidAsync(connection, cancellationToken).ConfigureAwait(false);
            return 0;
        }

        var since = now - Lookback;
        var result = await bank
            .GetTransactionsAsync(secret, connection.AccountIban, since, now, cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            if (result.Errors.Contains(DomainErrors.Bank.TokenInvalid))
            {
                await MarkTokenInvalidAsync(connection, cancellationToken).ConfigureAwait(false);
            }

            return 0;
        }

        var baseline = connection.CreatedAt - BaselineTolerance;

        var candidates = result.Value
            .Where(t => t.OccurredAt >= baseline)
            .GroupBy(t => t.ExternalId)
            .Select(g => g.First())
            .ToList();

        var fresh = await FilterNewAsync(connection.CreatedBy, candidates, cancellationToken).ConfigureAwait(false);

        connection.LastSyncAt = now;

        var notifications = fresh.Select(t => ToNotification(connection, t)).ToList();

        if (notifications.Count > 0)
        {
            notificationRepository.CreateRange(notifications);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information(
            "Bank sync for {User} ({Label}): fetched {Fetched}, new {New}",
            connection.CreatedBy,
            connection.Label,
            result.Value.Count,
            notifications.Count);

        if (notifications.Count > 0)
        {
            await PublishAsync(connection.CreatedBy, notifications, cancellationToken).ConfigureAwait(false);
            await SendEmailAsync(connection, fresh, cancellationToken).ConfigureAwait(false);
        }

        return notifications.Count;
    }

    private async Task PublishAsync(string createdBy, List<BankNotification> notifications, CancellationToken cancellationToken)
    {
        try
        {
            await publisher.PublishAsync(createdBy, notifications.ToResponses(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Warning(ex, "Failed to push bank notifications to user {User}", createdBy);
        }
    }

    private async Task<List<BankTransaction>> FilterNewAsync(
        string createdBy,
        List<BankTransaction> candidates,
        CancellationToken cancellationToken)
    {
        if (candidates.Count == 0)
        {
            return candidates;
        }

        var ids = candidates.Select(t => t.ExternalId).ToList();
        var known = (await notificationRepository
            .GetAllAsync(new BankNotificationsByExternalIdsSpecification(createdBy, ids), cancellationToken)
            .ConfigureAwait(false)).Select(n => n.ExternalId).ToHashSet(StringComparer.Ordinal);

        return candidates.Where(t => !known.Contains(t.ExternalId)).ToList();
    }

    private async Task MarkTokenInvalidAsync(BankConnection connection, CancellationToken cancellationToken)
    {
        connection.Status = BankConnectionStatus.TokenInvalid;

        var issue = new BankNotification
            {
                Kind = BankNotificationKind.ConnectionIssue,
                Direction = BankTransactionDirection.None,
                Type = BankTransactionType.None,
                AccountLabel = Truncate(connection.Label, 64),
                ExternalId = $"token-invalid:{Guid.NewGuid():N}",
                OccurredAt = dateTimeProvider.UtcNow,
                CreatedBy = connection.CreatedBy
            };

        notificationRepository.CreateRange([issue]);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Warning("Bank token is invalid for user {User}", connection.CreatedBy);

        await PublishAsync(connection.CreatedBy, [issue], cancellationToken).ConfigureAwait(false);
    }

    private static BankNotification ToNotification(BankConnection connection, BankTransaction transaction)
        => new()
        {
            Kind = BankNotificationKind.Transaction,
            Direction = transaction.Direction,
            Type = BankTransactionClassifier.Classify(transaction.Direction, transaction.Counterparty, transaction.Purpose),
            AccountLabel = Truncate(connection.Label, 64),
            ExternalId = Truncate(transaction.ExternalId, 128)!,
            Amount = transaction.Amount,
            Currency = Truncate(transaction.Currency, 8),
            Counterparty = Truncate(transaction.Counterparty, 256),
            Purpose = Truncate(transaction.Purpose, 512),
            OccurredAt = transaction.OccurredAt,
            CreatedBy = connection.CreatedBy
        };

    private async Task SendEmailAsync(BankConnection connection, List<BankTransaction> transactions, CancellationToken cancellationToken)
    {
        var createdBy = connection.CreatedBy;
        var settings = await userSettingsRepository.GetAsync(createdBy, cancellationToken).ConfigureAwait(false);

        if (settings is not { BankAlertsEmailEnabled: true, EmailNotificationsEnabled: true }
            || string.IsNullOrWhiteSpace(settings.Email))
        {
            return;
        }

        var rows = string.Concat(transactions.Select(t =>
            $"<li>{TypeLabel(BankTransactionClassifier.Classify(t.Direction, t.Counterparty, t.Purpose))}"
            + $" <strong>{(t.Direction == BankTransactionDirection.Incoming ? "+" : "-")}{t.Amount.ToString("N2", CultureInfo.InvariantCulture)} {WebUtility.HtmlEncode(t.Currency)}</strong>"
            + $" {WebUtility.HtmlEncode(t.Counterparty ?? string.Empty)}"
            + $" <span style=\"color:#888\">{WebUtility.HtmlEncode(t.Purpose ?? string.Empty)}</span></li>"));

        var html = $"""
            <h2>New bank transactions ({WebUtility.HtmlEncode(connection.Label)})</h2>
            <ul>{rows}</ul>
            <br/>
            <p style="color:#888;font-size:12px">This is an automated alert from Deed Finance.</p>
            """;

        try
        {
            await emailService.SendAsync(settings.Email, "Deed — New bank transactions", html, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Error(ex, "Failed to send bank alert e-mail for user {User}", createdBy);
        }
    }

    private static string TypeLabel(BankTransactionType type)
        => type switch
        {
            BankTransactionType.TopUp => "Top-up",
            BankTransactionType.Withdrawal => "Withdrawal",
            BankTransactionType.Transfer => "Transfer",
            _ => "Payment"
        };

    private static string? Truncate(string? value, int max)
        => value is null || value.Length <= max ? value : value[..max];
}
