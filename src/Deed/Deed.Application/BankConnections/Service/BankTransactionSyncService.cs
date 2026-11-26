using System.Globalization;
using System.Net;
using Deed.Application.Abstractions.Services;
using Deed.Application.BankConnections.Responses;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.BankNotifications;
using Deed.Application.BankNotifications.Import;
using Deed.Application.BankNotifications.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.BankConnections.Service;

public sealed class BankTransactionSyncService(
    IBankConnectionRepository connectionRepository,
    IBankNotificationRepository notificationRepository,
    IUserSettingsRepository userSettingsRepository,
    IEnumerable<IBankStatementProvider> providers,
    IBankTransactionImporter importer,
    ITokenProtector tokenProtector,
    IEmailService emailService,
    IBankNotificationPublisher publisher,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IBankTransactionSyncService
{
    private static readonly TimeSpan Lookback = TimeSpan.FromDays(2);
    private static readonly TimeSpan ChunkWindow = TimeSpan.FromDays(30);
    private static readonly TimeSpan BaselineTolerance = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan SilenceLimit = TimeSpan.FromDays(730);
    private const int MaxBackfillFailures = 10;
    private const int MaxExternalIdLength = 128;
    private static readonly DateTimeOffset EarliestPossible = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public Task<int> SyncAsync(CancellationToken cancellationToken)
        => SyncAsync(new ActiveBankConnectionsSpecification(), force: false, cancellationToken);

    public Task<int> SyncUserAsync(string createdBy, CancellationToken cancellationToken)
        => SyncAsync(new ActiveBankConnectionsSpecification(createdBy), force: true, cancellationToken);

    private async Task<int> SyncAsync(ActiveBankConnectionsSpecification specification, bool force, CancellationToken cancellationToken)
    {
        var connections = (await connectionRepository
            .GetAllAsync(specification, cancellationToken)
            .ConfigureAwait(false)).ToList();

        var created = 0;

        foreach (var connection in connections)
        {
            if (!force && IsFresh(connection))
            {
                continue;
            }

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

    private bool IsFresh(BankConnection connection)
        => ProviderFor(connection) is { } provider
            && provider.MinSyncInterval > TimeSpan.Zero
            && dateTimeProvider.UtcNow - connection.LastSyncAt < provider.MinSyncInterval;

    private async Task<int> SyncConnectionAsync(BankConnection connection, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var secret = tokenProtector.Unprotect(connection.EncryptedToken);

        if (secret is null)
        {
            await MarkTokenInvalidAsync(connection, cancellationToken).ConfigureAwait(false);
            return 0;
        }

        var provider = ProviderFor(connection);

        if (provider is null)
        {
            return 0;
        }

        if (await LearnCurrencyAsync(connection, provider, secret, cancellationToken).ConfigureAwait(false))
        {
            return 0;
        }

        var result = await provider
            .GetTransactionsAsync(secret, AccountOf(connection), now - Lookback, now, cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            await HandleFailureAsync(connection, result.Errors, cancellationToken).ConfigureAwait(false);

            return 0;
        }

        var baseline = connection.CreatedAt - BaselineTolerance;

        var candidates = InAccountCurrency(result.Value, connection.Currency)
            .Where(t => t.OccurredAt >= baseline)
            .GroupBy(t => t.ExternalId, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        var fresh = await FilterNewAsync(connection.CreatedBy, candidates, cancellationToken).ConfigureAwait(false);

        connection.LastSyncAt = now;

        var status = connection.CapitalId is null ? BankImportStatus.None : BankImportStatus.Pending;

        var notifications = fresh
            .Select(t => ToNotification(connection.CreatedBy, connection.Label, connection.Id, t, isRead: false, status))
            .ToList();

        if (notifications.Count > 0)
        {
            if (connection.AutoImport)
            {
                await AutoImportAsync(connection, notifications, cancellationToken).ConfigureAwait(false);
            }

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

    public async Task<BankHistoryImportResponse> ImportHistoryAsync(
        string createdBy,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var connections = (await connectionRepository
            .GetAllAsync(new ActiveBankConnectionsSpecification(createdBy), cancellationToken)
            .ConfigureAwait(false)).ToList();

        foreach (var connection in connections)
        {
            connection.BackfillFrom = from;
            connection.BackfillCursor = to;
            connection.BackfillFailures = 0;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new BankHistoryImportResponse(connections.Count > 0);
    }

    public async Task<int> BackfillAsync(CancellationToken cancellationToken)
    {
        var connections = (await connectionRepository
            .GetAllAsync(new BackfillBankConnectionsSpecification(), cancellationToken)
            .ConfigureAwait(false)).ToList();

        var imported = 0;

        foreach (var connection in connections)
        {
            var provider = ProviderFor(connection);
            var chunks = Math.Max(provider?.MaxChunksPerRun ?? 1, 1);

            for (var chunk = 0; chunk < chunks && connection.BackfillCursor is not null; chunk++)
            {
                var cursorBefore = connection.BackfillCursor;

                try
                {
                    imported += await BackfillConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Log.Error(ex, "Failed to backfill bank history for user {User}", connection.CreatedBy);
                    break;
                }

                if (connection.BackfillCursor is null || connection.BackfillCursor == cursorBefore)
                {
                    break;
                }

                if (provider is not null && provider.ChunkDelay > TimeSpan.Zero && chunk + 1 < chunks)
                {
                    await Task.Delay(provider.ChunkDelay, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        return imported;
    }

    private async Task<int> BackfillConnectionAsync(BankConnection connection, CancellationToken cancellationToken)
    {
        if (connection.BackfillCursor is not { } cursor || connection.BackfillFrom is not { } floor || cursor <= floor)
        {
            await FinishBackfillAsync(connection, cancellationToken).ConfigureAwait(false);
            return 0;
        }

        var secret = tokenProtector.Unprotect(connection.EncryptedToken);
        var provider = ProviderFor(connection);

        if (secret is null || provider is null)
        {
            connection.BackfillFrom = null;
            connection.BackfillCursor = null;

            if (secret is null)
            {
                await MarkTokenInvalidAsync(connection, cancellationToken).ConfigureAwait(false);
            }

            return 0;
        }

        if (await LearnCurrencyAsync(connection, provider, secret, cancellationToken).ConfigureAwait(false))
        {
            return 0;
        }

        var start = cursor - ChunkWindow > floor ? cursor - ChunkWindow : floor;

        var result = await provider
            .GetTransactionsAsync(secret, AccountOf(connection), start, cursor, cancellationToken)
            .ConfigureAwait(false);

        var state = await connectionRepository.GetBackfillStateAsync(connection.Id, cancellationToken).ConfigureAwait(false);

        if (state is null || state.IsExcluded || state.Cursor != cursor)
        {
            Log.Information("Bank backfill for {User} ({Label}) was changed while it ran, the fetched chunk is dropped", connection.CreatedBy, connection.Label);

            return 0;
        }

        if (!result.IsSuccess)
        {
            if (!result.Errors.Contains(DomainErrors.Bank.RateLimited) && !result.Errors.Contains(DomainErrors.Bank.TokenInvalid))
            {
                connection.BackfillFailures++;

                Log.Warning(
                    "Bank backfill for {User} ({Label}) failed {Count} time(s): {Reason}",
                    connection.CreatedBy,
                    connection.Label,
                    connection.BackfillFailures,
                    result.Errors.FirstOrDefault()?.Code);

                if (connection.BackfillFailures >= MaxBackfillFailures)
                {
                    connection.BackfillFrom = null;
                    connection.BackfillCursor = null;
                    connection.BackfillFailures = 0;
                }
            }

            await HandleFailureAsync(connection, result.Errors, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return 0;
        }

        var staged = await StageHistoryAsync(connection.CreatedBy, connection.Label, connection.Id, connection.Currency, result.Value, cancellationToken).ConfigureAwait(false);

        connection.BackfillFailures = 0;

        var next = start;

        if (provider.MaxItemsPerRequest is { } cap && result.Value.Count >= cap)
        {
            var oldest = result.Value.Min(t => t.OccurredAt);

            if (oldest > start && oldest < cursor)
            {
                next = oldest;
            }
        }

        if (next <= floor || result.Value.Count == 0 && await HasNoOlderHistoryAsync(connection, start, cancellationToken).ConfigureAwait(false))
        {
            connection.BackfillFrom = null;
            connection.BackfillCursor = null;
        }
        else
        {
            connection.BackfillCursor = next;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information(
            "Bank backfill for {User} ({Label}): imported {Imported}, cursor {Cursor}",
            connection.CreatedBy,
            connection.Label,
            staged,
            connection.BackfillCursor);

        return staged;
    }

    private async Task<bool> HasNoOlderHistoryAsync(BankConnection connection, DateTimeOffset start, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;

        var oldest = await notificationRepository
            .GetOldestAsync(new BankTransactionsSummarySpecification(connection.CreatedBy, connection.Id, EarliestPossible, now), cancellationToken)
            .ConfigureAwait(false);

        return start < (oldest ?? now) - SilenceLimit;
    }

    private async Task FinishBackfillAsync(BankConnection connection, CancellationToken cancellationToken)
    {
        connection.BackfillFrom = null;
        connection.BackfillCursor = null;

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> StageHistoryAsync(string createdBy, string label, int? connectionId, string? currency, IReadOnlyCollection<BankTransaction> transactions, CancellationToken cancellationToken)
    {
        var candidates = InAccountCurrency(transactions, currency)
            .GroupBy(t => t.ExternalId, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        var fresh = await FilterNewAsync(createdBy, candidates, cancellationToken).ConfigureAwait(false);

        if (fresh.Count > 0)
        {
            notificationRepository.CreateRange(fresh.Select(t => ToNotification(createdBy, label, connectionId, t, isRead: true, BankImportStatus.None)));
        }

        return fresh.Count;
    }

    private IBankStatementProvider? ProviderFor(BankConnection connection)
        => providers.FirstOrDefault(p => p.Provider == connection.Provider);

    private static string AccountOf(BankConnection connection)
        => connection.ProviderAccountId ?? connection.AccountIban;

    private async Task HandleFailureAsync(
        BankConnection connection,
        IList<Error> errors,
        CancellationToken cancellationToken)
    {
        if (errors.Contains(DomainErrors.Bank.TokenInvalid))
        {
            await MarkTokenInvalidAsync(connection, cancellationToken).ConfigureAwait(false);
        }
        else if (errors.Contains(DomainErrors.Bank.RateLimited))
        {
            Log.Information("Bank rate limit reached for {User} ({Label}), will retry on the next run", connection.CreatedBy, connection.Label);
        }
    }

    private async Task AutoImportAsync(BankConnection connection, List<BankNotification> notifications, CancellationToken cancellationToken)
    {
        foreach (var notification in notifications.Where(n => n.ImportStatus == BankImportStatus.Pending))
        {
            var imported = await importer.ImportAsync(connection, notification, null, cancellationToken).ConfigureAwait(false);

            if (!imported.IsSuccess)
            {
                Log.Information(
                    "Bank transaction {ExternalId} of {User} was not auto-imported: {Reason}",
                    notification.ExternalId,
                    connection.CreatedBy,
                    imported.Errors.FirstOrDefault()?.Code);
            }
        }
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

        var ids = candidates.Select(t => StoredId(t)).ToList();
        var known = (await notificationRepository
            .GetExternalIdsAsync(new BankNotificationsByExternalIdsSpecification(createdBy, ids), cancellationToken)
            .ConfigureAwait(false)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return candidates.Where(t => !known.Contains(StoredId(t))).ToList();
    }

    private static IEnumerable<BankTransaction> InAccountCurrency(IEnumerable<BankTransaction> transactions, string? currency)
        => string.IsNullOrWhiteSpace(currency)
            ? transactions
            : transactions.Select(t => t with { Currency = currency });

    private async Task<bool> LearnCurrencyAsync(BankConnection connection, IBankStatementProvider provider, string secret, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(connection.Currency) || provider is not IMonobankService monobank || connection.ProviderAccountId is null)
        {
            return false;
        }

        var accounts = await monobank.GetAccountsAsync(secret, cancellationToken).ConfigureAwait(false);

        if (!accounts.IsSuccess)
        {
            return true;
        }

        var account = accounts.Value.FirstOrDefault(a => a.Id == connection.ProviderAccountId);

        if (account is null || string.IsNullOrWhiteSpace(account.Currency))
        {
            return true;
        }

        connection.Currency = account.Currency;

        await notificationRepository.SetCurrencyAsync(connection.CreatedBy, connection.Id, account.Currency, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Bank account currency learned for {User} ({Label}): {Currency}", connection.CreatedBy, connection.Label, account.Currency);

        return true;
    }

    private static string StoredId(BankTransaction transaction)
        => Truncate(transaction.ExternalId, MaxExternalIdLength)!;

    private async Task MarkTokenInvalidAsync(BankConnection connection, CancellationToken cancellationToken)
    {
        connection.Status = BankConnectionStatus.TokenInvalid;
        connection.BackfillFrom = null;
        connection.BackfillCursor = null;

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

    private static BankNotification ToNotification(
        string createdBy,
        string label,
        int? connectionId,
        BankTransaction transaction,
        bool isRead,
        BankImportStatus importStatus)
    {
        var type = BankTransactionClassifier.Classify(transaction.Direction, transaction.Counterparty, transaction.Purpose);

        return new()
        {
            Kind = BankNotificationKind.Transaction,
            Direction = transaction.Direction,
            Type = type,
            AccountLabel = Truncate(label, 64),
            ExternalId = StoredId(transaction),
            Amount = transaction.Amount,
            Currency = Truncate(transaction.Currency, 8),
            Counterparty = Truncate(transaction.Counterparty, 256),
            Purpose = Truncate(transaction.Purpose, 512),
            OccurredAt = transaction.OccurredAt,
            IsRead = isRead,
            BankConnectionId = connectionId,
            ImportStatus = type == BankTransactionType.Exchange ? BankImportStatus.None : importStatus,
            CreatedBy = createdBy
        };
    }

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
            $"<li>{BankTransactionClassifier.Classify(t.Direction, t.Counterparty, t.Purpose).ToLabel()}"
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

    private static string? Truncate(string? value, int max)
        => value is null || value.Length <= max ? value : value[..max];
}
