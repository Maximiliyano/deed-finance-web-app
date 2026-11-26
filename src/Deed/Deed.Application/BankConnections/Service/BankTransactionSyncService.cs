using System.Globalization;
using System.Net;
using Deed.Application.Abstractions.Services;
using Deed.Application.BankConnections.Specifications;
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
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IBankTransactionSyncService
{
    private static readonly TimeSpan Overlap = TimeSpan.FromMinutes(10);

    public async Task SyncAsync(CancellationToken cancellationToken)
    {
        var connections = (await connectionRepository
            .GetAllAsync(new ActiveBankConnectionsSpecification(), cancellationToken)
            .ConfigureAwait(false)).ToList();

        foreach (var connection in connections)
        {
            try
            {
                await SyncConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(ex, "Failed to sync bank transactions for user {User}", connection.CreatedBy);
            }
        }
    }

    private async Task SyncConnectionAsync(BankConnection connection, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var token = tokenProtector.Unprotect(connection.EncryptedToken);

        if (token is null)
        {
            await MarkTokenInvalidAsync(connection, cancellationToken).ConfigureAwait(false);
            return;
        }

        var since = connection.LastSyncAt - Overlap;
        var result = await bank
            .GetTransactionsAsync(token, connection.AccountIban, since, now, cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            if (result.Errors.Contains(DomainErrors.Bank.TokenInvalid))
            {
                await MarkTokenInvalidAsync(connection, cancellationToken).ConfigureAwait(false);
            }

            return;
        }

        var candidates = result.Value
            .Where(t => t.OccurredAt >= since)
            .GroupBy(t => t.ExternalId)
            .Select(g => g.First())
            .ToList();

        var fresh = await FilterNewAsync(connection.CreatedBy, candidates, cancellationToken).ConfigureAwait(false);

        connection.LastSyncAt = now;

        if (fresh.Count > 0)
        {
            notificationRepository.CreateRange(fresh.Select(t => ToNotification(connection, t)));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (fresh.Count > 0)
        {
            Log.Information("Bank sync created {Count} notification(s) for user {User}", fresh.Count, connection.CreatedBy);
            await SendEmailAsync(connection.CreatedBy, fresh, cancellationToken).ConfigureAwait(false);
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

        notificationRepository.CreateRange([
            new BankNotification
            {
                Kind = BankNotificationKind.ConnectionIssue,
                Direction = BankTransactionDirection.None,
                ExternalId = $"token-invalid:{Guid.NewGuid():N}",
                OccurredAt = dateTimeProvider.UtcNow,
                CreatedBy = connection.CreatedBy
            }
        ]);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Warning("Bank token is invalid for user {User}", connection.CreatedBy);
    }

    private static BankNotification ToNotification(BankConnection connection, BankTransaction transaction)
        => new()
        {
            Kind = BankNotificationKind.Transaction,
            Direction = transaction.Direction,
            ExternalId = Truncate(transaction.ExternalId, 128)!,
            Amount = transaction.Amount,
            Currency = Truncate(transaction.Currency, 8),
            Counterparty = Truncate(transaction.Counterparty, 256),
            Purpose = Truncate(transaction.Purpose, 512),
            OccurredAt = transaction.OccurredAt,
            CreatedBy = connection.CreatedBy
        };

    private async Task SendEmailAsync(string createdBy, List<BankTransaction> transactions, CancellationToken cancellationToken)
    {
        var settings = await userSettingsRepository.GetAsync(createdBy, cancellationToken).ConfigureAwait(false);

        if (settings is not { BankAlertsEmailEnabled: true, EmailNotificationsEnabled: true }
            || string.IsNullOrWhiteSpace(settings.Email))
        {
            return;
        }

        var rows = string.Concat(transactions.Select(t =>
            $"<li><strong>{(t.Direction == BankTransactionDirection.Incoming ? "+" : "-")}{t.Amount.ToString("N2", CultureInfo.InvariantCulture)} {WebUtility.HtmlEncode(t.Currency)}</strong>"
            + $" {WebUtility.HtmlEncode(t.Counterparty ?? string.Empty)}"
            + $" <span style=\"color:#888\">{WebUtility.HtmlEncode(t.Purpose ?? string.Empty)}</span></li>"));

        var html = $"""
            <h2>New bank transactions</h2>
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

    private static string? Truncate(string? value, int max)
        => value is null || value.Length <= max ? value : value[..max];
}
