using Deed.Application.Abstractions.Messaging;
using Deed.Application.Incomes.Responses;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Incomes.Queries.GetAll;

internal sealed class GetIncomesQueryHandler(
    IIncomeRepository repository)
    : IQueryHandler<GetIncomesQuery, IEnumerable<IncomeResponse>>
{
    public async Task<Result<IEnumerable<IncomeResponse>>> Handle(GetIncomesQuery query, CancellationToken cancellationToken)
    {
        var incomes = await repository.GetAllAsync().ConfigureAwait(false);

        var incomeResponses = incomes.ToResponses();

        return Result.Success(incomeResponses);
    }
}
