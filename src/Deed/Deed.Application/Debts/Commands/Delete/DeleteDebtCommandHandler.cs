using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Debts.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.Debts.Commands.Delete;

internal sealed class DeleteDebtCommandHandler(
    IDebtRepository repository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<DeleteDebtCommand>
{
    public async Task<Result> Handle(DeleteDebtCommand command, CancellationToken cancellationToken)
    {
        var debt = await repository
            .GetAsync(new DebtByIdSpecification(command.Id, user.Name), cancellationToken)
            .ConfigureAwait(false);

        if (debt is null)
        {
            return Result.Failure(DomainErrors.General.NotFound("debt"));
        }

        repository.Delete(debt);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Debt {Id} deleted", command.Id);

        return Result.Success();
    }
}
