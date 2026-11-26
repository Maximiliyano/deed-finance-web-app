using Deed.Application.Abstractions.Services;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using Serilog;

namespace Deed.Application.BankConnections.Service;

public sealed class BankAccountRefreshService(
    IBankConnectionRepository connectionRepository,
    IBankAccountRepository accountRepository,
    IPrivatAutoclientService privat,
    ITokenProtector tokenProtector,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : IBankAccountRefreshService
{
    public Task<int> RefreshAsync(CancellationToken cancellationToken)
        => RefreshAsync(new ActiveBankConnectionsSpecification(includeExcluded: true, provider: BankProvider.PrivatAutoclient), cancellationToken);

    public Task<int> RefreshUserAsync(string createdBy, CancellationToken cancellationToken)
        => RefreshAsync(new ActiveBankConnectionsSpecification(createdBy, includeExcluded: true, provider: BankProvider.PrivatAutoclient), cancellationToken);

    private async Task<int> RefreshAsync(ActiveBankConnectionsSpecification specification, CancellationToken cancellationToken)
    {
        var connections = (await connectionRepository
            .GetAllAsync(specification, cancellationToken)
            .ConfigureAwait(false))
            .ToList();

        var refreshed = 0;

        foreach (var group in connections.GroupBy(c => c.CreatedBy))
        {
            try
            {
                refreshed += await RefreshAccountsOfUserAsync(group.Key, group.ToList(), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(ex, "Failed to refresh bank accounts for user {User}", group.Key);
            }
        }

        return refreshed;
    }

    private async Task<int> RefreshAccountsOfUserAsync(string createdBy, List<BankConnection> connections, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;

        var existing = (await accountRepository
            .GetAllAsync(new BankAccountsByUserSpecification(createdBy, tracking: true), cancellationToken)
            .ConfigureAwait(false)).ToList();

        var byIban = existing.ToDictionary(a => a.Iban, StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var tokens = new HashSet<string>(StringComparer.Ordinal);
        var complete = true;
        var refreshed = 0;

        foreach (var connection in connections)
        {
            var secret = tokenProtector.Unprotect(connection.EncryptedToken);

            if (secret is null)
            {
                complete = false;
                continue;
            }

            if (!tokens.Add(secret))
            {
                continue;
            }

            var result = await privat.GetAccountsAsync(secret, cancellationToken).ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                complete = false;

                Log.Warning("Could not load bank accounts for {User} ({Label}): {Reason}", createdBy, connection.Label, result.Errors.FirstOrDefault()?.Code);

                continue;
            }

            foreach (var account in result.Value)
            {
                seen.Add(account.Iban);
                refreshed++;

                if (!string.IsNullOrWhiteSpace(account.Currency))
                {
                    foreach (var linked in connections.Where(c => string.Equals(c.AccountIban, account.Iban, StringComparison.OrdinalIgnoreCase)))
                    {
                        linked.Currency = account.Currency;
                    }
                }

                if (byIban.TryGetValue(account.Iban, out var row))
                {
                    row.Currency = account.Currency;
                    row.Name = Truncate(account.Name, 128);
                    row.Balance = account.Balance;
                    row.SourceConnectionId = connection.Id;
                    row.LastRefreshedAt = now;
                }
                else
                {
                    var created = new BankAccount
                    {
                        Provider = BankProvider.PrivatAutoclient,
                        Iban = account.Iban,
                        Currency = account.Currency,
                        Name = Truncate(account.Name, 128),
                        Balance = account.Balance,
                        SourceConnectionId = connection.Id,
                        LastRefreshedAt = now,
                        CreatedBy = createdBy
                    };

                    accountRepository.Create(created);
                    byIban[account.Iban] = created;
                }
            }
        }

        if (complete)
        {
            foreach (var stale in existing.Where(a => !seen.Contains(a.Iban)
                && !connections.Any(c => string.Equals(c.AccountIban, a.Iban, StringComparison.OrdinalIgnoreCase))))
            {
                accountRepository.Delete(stale);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Bank accounts for {User}: {Count} refreshed", createdBy, refreshed);

        return refreshed;
    }

    private static string? Truncate(string? value, int max)
        => value is null || value.Length <= max ? value : value[..max];
}
