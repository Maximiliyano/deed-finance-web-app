using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Goals.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.Goals.Commands.Create;

internal sealed class CreateGoalCommandHandler(
    IUser user,
    IGoalRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateGoalCommand, int>
{
    public async Task<Result<int>> Handle(CreateGoalCommand command, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated)
        {
            var count = await repository.CountAsync(
                new GoalsByUserSpecification(user.Name!), cancellationToken).ConfigureAwait(false);

            if (count >= AnonymousConstants.EntityLimit)
            {
                return Result.Failure<int>(DomainErrors.Anonymous.LimitReached);
            }
        }

        var goal = command.ToEntity();

        repository.Create(goal);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Goal {Id} created", goal.Id);

        return Result.Success(goal.Id);
    }
}
