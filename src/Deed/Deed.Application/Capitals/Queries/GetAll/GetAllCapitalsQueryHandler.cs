using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Responses;
using Deed.Application.Capitals.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Capitals.Queries.GetAll;

internal sealed class GetAllCapitalsQueryHandler(
    IUser user,
    ICapitalRepository repository
) : IQueryHandler<GetAllCapitalsQuery, IEnumerable<CapitalResponse>>
{
    public async Task<Result<IEnumerable<CapitalResponse>>> Handle(GetAllCapitalsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(user.Name);

        var capitals = await repository
            .GetAllAsync(new CapitalsByQueryParamsSpecification(user.Name, query.SearchTerm, query.SortBy, query.SortDirection, query.FilterBy))
            .ConfigureAwait(false);

        var capitalResponses = capitals.ToResponses();

        return Result.Success(capitalResponses);
    }
}
