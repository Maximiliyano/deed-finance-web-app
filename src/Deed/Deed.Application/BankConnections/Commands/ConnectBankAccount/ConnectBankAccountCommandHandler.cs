using Deed.Application.Abstractions.Messaging;
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

namespace Deed.Application.BankConnections.Commands.ConnectBankAccount;

internal sealed class ConnectBankAccountCommandHandler(
    IUser user,
    IBankAccountRepository accountRepository,
    IBankConnectionRepository connectionRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    IBankBackfillTrigger backfillTrigger)
    : ICommandHandler<ConnectBankAccountCommand>
{
    private const int MaxLabelLength = 64;
    private const int FullHistoryDays = 3650;

    public async Task<Result> Handle(ConnectBankAccountCommand command, CancellationToken cancellationToken)
    {
        var account = await accountRepository
            .GetAsync(new BankAccountByIdSpecification(command.Id, user.Name!), cancellationToken)
            .ConfigureAwait(false);

        if (account is null)
        {
            return Result.Failure(DomainErrors.Bank.AccountNotFound);
        }

        var existing = await connectionRepository
            .GetAsync(new BankConnectionByAccountSpecification(user.Name!, account.Provider, account.Iban), cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            return Result.Failure(DomainErrors.Bank.AccountAlreadyConnected);
        }

        var source = await connectionRepository
            .GetAsync(new BankConnectionByIdSpecification(account.SourceConnectionId, user.Name!), cancellationToken)
            .ConfigureAwait(false);

        if (source is null || source.Status != BankConnectionStatus.Active)
        {
            return Result.Failure(DomainErrors.Bank.NotConnected);
        }

        var label = BankConnectionExtensions.NormalizeLabel(command.Label) ?? DefaultLabel(source, account);

        var now = dateTimeProvider.UtcNow;

        connectionRepository.Create(new BankConnection
        {
            Provider = account.Provider,
            EncryptedToken = source.EncryptedToken,
            AccountIban = account.Iban,
            Label = label,
            Currency = string.IsNullOrWhiteSpace(account.Currency) ? null : account.Currency,
            Status = BankConnectionStatus.Active,
            LastSyncAt = now,
            BackfillFrom = now.AddDays(-FullHistoryDays),
            BackfillCursor = now
        });

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await backfillTrigger.TriggerAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Bank account {Id} connected for {User}", account.Id, user.Name);

        return Result.Success();
    }

    private static string DefaultLabel(BankConnection source, BankAccount account)
    {
        var label = $"{(string.IsNullOrWhiteSpace(account.Name) ? source.Label : account.Name)} {account.Iban[^4..]}";

        return label.Length <= MaxLabelLength ? label : label[..MaxLabelLength];
    }
}
