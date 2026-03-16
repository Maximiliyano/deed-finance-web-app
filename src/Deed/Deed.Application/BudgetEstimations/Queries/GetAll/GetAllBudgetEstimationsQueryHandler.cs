using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BudgetEstimations.Responses;
using Deed.Application.BudgetEstimations.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BudgetEstimations.Queries.GetAll;

internal sealed class GetAllBudgetEstimationsQueryHandler(IBudgetEstimationRepository repository, IUser user)
    : IQueryHandler<GetAllBudgetEstimationsQuery, IEnumerable<BudgetEstimationResponse>>
{
    public async Task<Result<IEnumerable<BudgetEstimationResponse>>> Handle(
        GetAllBudgetEstimationsQuery query,
        CancellationToken cancellationToken)
    {
        var estimations = await repository
            .GetAllAsync(new BudgetEstimationsByUserSpecification(user.Name!, includeCapital: true), cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(estimations.ToResponses());
    }
}
