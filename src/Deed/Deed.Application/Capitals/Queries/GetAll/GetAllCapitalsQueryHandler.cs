using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Responses;
using Deed.Application.Capitals.Specifications;
using Deed.Domain.Constants;
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
        var summaries = await repository
            .GetSummariesAsync(new CapitalsByQueryParamsSpecification(user?.Name, query.SearchTerm, query.SortBy, query.SortDirection, query.FilterBy), cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(SortByTotals(summaries.ToResponses(), query.SortBy, query.SortDirection));
    }

    private static IEnumerable<CapitalResponse> SortByTotals(IEnumerable<CapitalResponse> capitals, string? sortBy, string? sortDirection)
    {
        Func<CapitalResponse, decimal>? keySelector = sortBy switch
        {
            SortKeysConstants.Expenses => c => c.TotalExpense,
            SortKeysConstants.Incomes => c => c.TotalIncome,
            SortKeysConstants.TransfersIn => c => c.TotalTransferIn,
            SortKeysConstants.TransfersOut => c => c.TotalTransferOut,
            _ => null
        };

        if (keySelector is null)
        {
            return capitals;
        }

        return string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? capitals.OrderBy(keySelector)
            : capitals.OrderByDescending(keySelector);
    }
}
