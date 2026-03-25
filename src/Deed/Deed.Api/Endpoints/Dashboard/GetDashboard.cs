using Deed.Api.Extensions;
using Deed.Application.Dashboard;
using MediatR;

namespace Deed.Api.Endpoints.Dashboard;

internal sealed class GetDashboard : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/dashboard", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetDashboardQuery(), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(Dashboard));
    }
}
