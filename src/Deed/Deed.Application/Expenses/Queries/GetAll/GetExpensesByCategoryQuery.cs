using Deed.Application.Abstractions.Messaging;
using Deed.Application.Expenses.Responses;

namespace Deed.Application.Expenses.Queries.GetAll;

public sealed record GetExpensesByCategoryQuery(
    int? CapitalId)
    : IQuery<IEnumerable<CategoryExpenseResponse>>;
