using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Service;
using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Commands.ImportBankHistory;

internal sealed class ImportBankHistoryCommandHandler(IUser user, IBankTransactionSyncService syncService)
    : ICommandHandler<ImportBankHistoryCommand, int>
{
    public async Task<Result<int>> Handle(ImportBankHistoryCommand command, CancellationToken cancellationToken)
    {
        var imported = await syncService.ImportHistoryAsync(user.Name!, cancellationToken).ConfigureAwait(false);

        return Result.Success(imported);
    }
}
