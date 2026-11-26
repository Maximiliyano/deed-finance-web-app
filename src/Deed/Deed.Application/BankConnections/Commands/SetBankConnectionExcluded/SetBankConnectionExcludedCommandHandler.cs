using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Commands.SetBankConnectionExcluded;

internal sealed class SetBankConnectionExcludedCommandHandler(
    IUser user,
    IBankConnectionRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SetBankConnectionExcludedCommand>
{
    public async Task<Result> Handle(SetBankConnectionExcludedCommand command, CancellationToken cancellationToken)
    {
        var connection = await repository
            .GetAsync(new BankConnectionByIdSpecification(command.Id, user.Name!, tracking: true), cancellationToken)
            .ConfigureAwait(false);

        if (connection is null)
        {
            return Result.Failure(DomainErrors.Bank.NotConnected);
        }

        if (connection.IsExcluded == command.Excluded)
        {
            return Result.Success();
        }

        connection.IsExcluded = command.Excluded;

        if (command.Excluded)
        {
            connection.BackfillFrom = null;
            connection.BackfillCursor = null;
            connection.BackfillFailures = 0;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
