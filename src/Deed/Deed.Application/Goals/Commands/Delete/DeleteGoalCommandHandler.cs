using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Goals.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.Goals.Commands.Delete;

internal sealed class DeleteGoalCommandHandler(
    IGoalRepository repository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<DeleteGoalCommand>
{
    public async Task<Result> Handle(DeleteGoalCommand command, CancellationToken cancellationToken)
    {
        var goal = await repository
            .GetAsync(new GoalByIdSpecification(command.Id, user.Name), cancellationToken)
            .ConfigureAwait(false);

        if (goal is null)
        {
            return Result.Failure(DomainErrors.General.NotFound("goal"));
        }

        repository.Delete(goal);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Goal {Id} deleted", command.Id);

        return Result.Success();
    }
}
