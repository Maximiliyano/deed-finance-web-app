using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BudgetEstimations.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.BudgetEstimations.Commands.Delete;

internal sealed class DeleteBudgetEstimationCommandHandler(
    IBudgetEstimationRepository repository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<DeleteBudgetEstimationCommand>
{
    public async Task<Result> Handle(DeleteBudgetEstimationCommand command, CancellationToken cancellationToken)
    {
        var estimation = await repository
            .GetAsync(new BudgetEstimationByIdSpecification(command.Id, user.Name), cancellationToken)
            .ConfigureAwait(false);

        if (estimation is null)
        {
            return Result.Failure(DomainErrors.General.NotFound("budget estimation"));
        }

        repository.Delete(estimation);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("BudgetEstimation {Id} deleted", command.Id);

        return Result.Success();
    }
}
