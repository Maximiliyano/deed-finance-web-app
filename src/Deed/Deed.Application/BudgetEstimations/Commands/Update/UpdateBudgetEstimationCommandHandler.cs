using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BudgetEstimations.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.BudgetEstimations.Commands.Update;

internal sealed class UpdateBudgetEstimationCommandHandler(
    IBudgetEstimationRepository repository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<UpdateBudgetEstimationCommand>
{
    public async Task<Result> Handle(UpdateBudgetEstimationCommand command, CancellationToken cancellationToken)
    {
        var estimation = await repository
            .GetAsync(new BudgetEstimationByIdSpecification(command.Id, user.Name), cancellationToken)
            .ConfigureAwait(false);

        if (estimation is null)
        {
            return Result.Failure(DomainErrors.General.NotFound("budget estimation"));
        }

        estimation.ApplyUpdate(command);

        repository.Update(estimation);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("BudgetEstimation {Id} updated", command.Id);

        return Result.Success();
    }
}
