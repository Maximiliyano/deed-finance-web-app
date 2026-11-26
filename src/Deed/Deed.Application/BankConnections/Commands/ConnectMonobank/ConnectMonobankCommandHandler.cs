using Deed.Application.Abstractions.Messaging;
using Deed.Application.Abstractions.Services;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Service;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.BankConnections.Commands.ConnectMonobank;

internal sealed class ConnectMonobankCommandHandler(
    IUser user,
    IBankConnectionRepository repository,
    IMonobankService bank,
    IBankTransactionSyncService syncService,
    ITokenProtector tokenProtector,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ConnectMonobankCommand>
{
    private const string DefaultLabel = "Monobank";
    private const int HistoryDays = 30;
    private const int MaxIbanLength = 34;

    public async Task<Result> Handle(ConnectMonobankCommand command, CancellationToken cancellationToken)
    {
        var token = command.Token.Trim();
        var accountId = command.AccountId.Trim();
        var iban = BankConnectionExtensions.NormalizeIban(command.Iban);

        if (iban.Length == 0)
        {
            iban = accountId.Length <= MaxIbanLength ? accountId : accountId[..MaxIbanLength];
        }

        var now = dateTimeProvider.UtcNow;

        var check = await bank
            .GetTransactionsAsync(token, accountId, now.AddDays(-HistoryDays), now, cancellationToken)
            .ConfigureAwait(false);

        if (!check.IsSuccess && !check.Errors.Contains(DomainErrors.Bank.RateLimited))
        {
            return Result.Failure(check.Errors);
        }

        IReadOnlyCollection<BankTransaction> history = check.IsSuccess ? check.Value : [];

        var existing = await repository
            .GetAsync(new BankConnectionByAccountSpecification(user.Name!, BankProvider.Monobank, iban, tracking: true), cancellationToken)
            .ConfigureAwait(false);

        var encrypted = tokenProtector.Protect(token);
        var label = BankConnectionExtensions.NormalizeLabel(command.Label) ?? existing?.Label ?? DefaultLabel;

        BankConnection connection;

        if (existing is null)
        {
            connection = new BankConnection
            {
                Provider = BankProvider.Monobank,
                EncryptedToken = encrypted,
                AccountIban = iban,
                ProviderAccountId = accountId,
                Label = label,
                Status = BankConnectionStatus.Active,
                LastSyncAt = now
            };

            repository.Create(connection);
        }
        else
        {
            connection = existing;
            existing.EncryptedToken = encrypted;
            existing.ProviderAccountId = accountId;
            existing.Label = label;
            existing.Status = BankConnectionStatus.Active;
            existing.LastSyncAt = now;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var imported = await syncService
            .StageHistoryAsync(user.Name!, label, connection.Id, history, cancellationToken)
            .ConfigureAwait(false);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Monobank connection saved for {User}, imported {Imported} historical transactions", user.Name, imported);

        return Result.Success();
    }
}
