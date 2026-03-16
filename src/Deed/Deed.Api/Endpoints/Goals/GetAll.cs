using Deed.Api.Extensions;
using Deed.Application.Goals.Queries.GetAll;
using MediatR;

namespace Deed.Api.Endpoints.Goals;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/goals", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetAllGoalsQuery(), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(Goals));
    }
}
