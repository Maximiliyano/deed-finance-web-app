using Deed.Application.Abstractions.Messaging;
using Deed.Application.Incomes.Responses;

namespace Deed.Application.Incomes.Queries.GetAll;

public sealed record GetIncomesQuery(
    DateTime? PeriodStart = null,
    DateTime? PeriodEnd = null)
    : IQuery<IEnumerable<IncomeResponse>>;
