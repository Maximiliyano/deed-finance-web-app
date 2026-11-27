using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Service;
using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Commands.RefreshBankAccounts;

internal sealed class RefreshBankAccountsCommandHandler(IUser user, IBankAccountRefreshService refreshService)
    : ICommandHandler<RefreshBankAccountsCommand, int>
{
    public async Task<Result<int>> Handle(RefreshBankAccountsCommand command, CancellationToken cancellationToken)
    {
        var refreshed = await refreshService.RefreshUserAsync(user.Id, cancellationToken).ConfigureAwait(false);

        return Result.Success(refreshed);
    }
}
