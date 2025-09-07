using Deed.Api.Extensions;
using Deed.Application.Capitals.Queries.GetAll;
using Deed.Application.Capitals.Requests;
using MediatR;

namespace Deed.Api.Endpoints.Capitals;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/capitals", async (ISender sender, string? searchTerm, string? sortBy, string? sortDirection, CancellationToken ct) =>
            (await sender
                .Send(new GetAllCapitalsQuery(searchTerm, sortBy, sortDirection), ct))
                .Process())
            .WithTags(nameof(Capitals));
    }
}
