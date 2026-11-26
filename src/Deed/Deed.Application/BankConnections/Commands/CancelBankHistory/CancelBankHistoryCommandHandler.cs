using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.BankConnections.Commands.CancelBankHistory;

internal sealed class CancelBankHistoryCommandHandler(
    IUser user,
    IBankConnectionRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CancelBankHistoryCommand>
{
    public async Task<Result> Handle(CancelBankHistoryCommand command, CancellationToken cancellationToken)
    {
        var connections = (await repository
            .GetAllAsync(new BankConnectionByUserSpecification(user.Name!, tracking: true), cancellationToken)
            .ConfigureAwait(false))
            .Where(c => c.BackfillCursor is not null)
            .ToList();

        foreach (var connection in connections)
        {
            connection.BackfillFrom = null;
            connection.BackfillCursor = null;
            connection.BackfillFailures = 0;
        }

        if (connections.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            Log.Information("Bank history import cancelled for {User} on {Count} connection(s)", user.Name, connections.Count);
        }

        return Result.Success();
    }
}
