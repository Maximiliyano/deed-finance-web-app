using Deed.Application.Abstractions.Messaging;
using Deed.Application.Capitals.Responses;
using Deed.Application.Capitals.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Capitals.Queries.GetAll;

internal sealed class GetAllCapitalsQueryHandler(
    ICapitalRepository repository)
    : IQueryHandler<GetAllCapitalsQuery, IEnumerable<CapitalResponse>>
{
    public async Task<Result<IEnumerable<CapitalResponse>>> Handle(GetAllCapitalsQuery query, CancellationToken cancellationToken)
    {
        var capitals = await repository
            .GetAllAsync(new CapitalsByQueryParamsSpecification(query.SearchTerm, query.SortBy, query.SortDirection, query.FilterBy))
            .ConfigureAwait(false);

        var capitalResponses = capitals.ToResponses();

        return Result.Success(capitalResponses);
    }
}
