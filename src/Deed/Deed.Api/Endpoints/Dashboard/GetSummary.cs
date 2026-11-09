using Deed.Api.Extensions;
using Deed.Application.Dashboard.Queries.GetSummary;
using MediatR;

namespace Deed.Api.Endpoints.Dashboard;

internal sealed class GetSummary : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/dashboard/summary", async (string? currency, int[]? capitalIds, DateTime? periodStart, DateTime? periodEnd, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new GetDashboardSummaryQuery(currency, capitalIds, periodStart, periodEnd), ct))
                .Process())
            .AllowAnonymous()
            .WithTags(nameof(Dashboard));
    }
}
