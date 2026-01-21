using Deed.Application.Capitals.Responses;
using Deed.Application.Categories.Response;

namespace Deed.Application.Incomes.Responses;

public sealed record IncomesResponse(
    IEnumerable<IncomeResponse> Incomes,
    IEnumerable<CategoryResponse> Categories,
    IEnumerable<CapitalResponse> Capitals
);
