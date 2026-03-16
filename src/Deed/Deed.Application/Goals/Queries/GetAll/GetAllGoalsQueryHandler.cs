using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Goals.Responses;
using Deed.Application.Goals.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Goals.Queries.GetAll;

internal sealed class GetAllGoalsQueryHandler(IGoalRepository repository, IUser user)
    : IQueryHandler<GetAllGoalsQuery, IEnumerable<GoalResponse>>
{
    public async Task<Result<IEnumerable<GoalResponse>>> Handle(
        GetAllGoalsQuery query,
        CancellationToken cancellationToken)
    {
        var goals = await repository
            .GetAllAsync(new GoalsByUserSpecification(user.Name!), cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(goals.ToResponses());
    }
}
