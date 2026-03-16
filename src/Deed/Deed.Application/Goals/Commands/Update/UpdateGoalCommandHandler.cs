using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Goals.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.Goals.Commands.Update;

internal sealed class UpdateGoalCommandHandler(
    IGoalRepository repository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<UpdateGoalCommand>
{
    public async Task<Result> Handle(UpdateGoalCommand command, CancellationToken cancellationToken)
    {
        var goal = await repository
            .GetAsync(new GoalByIdSpecification(command.Id, user.Name), cancellationToken)
            .ConfigureAwait(false);

        if (goal is null)
        {
            return Result.Failure(DomainErrors.General.NotFound("goal"));
        }

        goal.ApplyUpdate(command);

        repository.Update(goal);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Goal {Id} updated", command.Id);

        return Result.Success();
    }
}
