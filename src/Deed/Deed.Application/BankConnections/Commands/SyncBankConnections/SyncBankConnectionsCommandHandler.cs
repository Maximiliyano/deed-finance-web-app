using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Service;
using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Commands.SyncBankConnections;

internal sealed class SyncBankConnectionsCommandHandler(IUser user, IBankTransactionSyncService syncService)
    : ICommandHandler<SyncBankConnectionsCommand, int>
{
    public async Task<Result<int>> Handle(SyncBankConnectionsCommand command, CancellationToken cancellationToken)
    {
        var created = await syncService.SyncUserAsync(user.Id, cancellationToken).ConfigureAwait(false);

        return Result.Success(created);
    }
}
