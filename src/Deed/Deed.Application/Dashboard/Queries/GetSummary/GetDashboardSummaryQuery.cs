using Deed.Application.Abstractions.Messaging;
using Deed.Application.Dashboard.Responses;

namespace Deed.Application.Dashboard.Queries.GetSummary;

public sealed record GetDashboardSummaryQuery(
    string? Currency = null,
    IReadOnlyList<int>? CapitalIds = null,
    DateTime? PeriodStart = null,
    DateTime? PeriodEnd = null)
    : IQuery<DashboardSummaryResponse>;
