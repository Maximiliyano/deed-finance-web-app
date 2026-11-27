using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.BankConnections.Commands.DisconnectBankProvider;

internal sealed class DisconnectBankProviderCommandHandler(
    IUser user,
    IBankConnectionRepository connectionRepository,
    IBankAccountRepository accountRepository,
    IBankNotificationRepository notificationRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DisconnectBankProviderCommand>
{
    public async Task<Result> Handle(DisconnectBankProviderCommand command, CancellationToken cancellationToken)
    {
        var provider = Enum.Parse<BankProvider>(command.Provider, true);

        var connections = (await connectionRepository
            .GetAllAsync(new BankConnectionByUserSpecification(user.Id, tracking: true), cancellationToken)
            .ConfigureAwait(false))
            .Where(c => c.Provider == provider)
            .ToList();

        var accounts = (await accountRepository
            .GetAllAsync(new BankAccountsByUserSpecification(user.Id, tracking: true), cancellationToken)
            .ConfigureAwait(false))
            .Where(a => a.Provider == provider)
            .ToList();

        if (connections.Count == 0 && accounts.Count == 0)
        {
            return Result.Failure(DomainErrors.Bank.NotConnected);
        }

        foreach (var connection in connections)
        {
            connectionRepository.Delete(connection);
        }

        foreach (var account in accounts)
        {
            accountRepository.Delete(account);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var removed = await notificationRepository
            .DeleteByConnectionsAsync(user.Id, [.. connections.Select(c => c.Id)], cancellationToken)
            .ConfigureAwait(false);

        Log.Information(
            "{Provider} disconnected for {User}: {Connections} connection(s), {Accounts} account(s), {Transactions} stored transaction(s) removed",
            provider,
            user.Id,
            connections.Count,
            accounts.Count,
            removed);

        return Result.Success();
    }
}
