using Deed.Api.Extensions;
using Deed.Application.Exchanges.Queries.GetLatest;
using MediatR;

namespace Deed.Api.Endpoints.Exchanges;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/exchanges", async (ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new GetLatestExchangeQuery(), ct))
                .Process())
            .WithTags(nameof(Exchanges));
    }
}
