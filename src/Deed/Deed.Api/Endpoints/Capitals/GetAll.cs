using Deed.Api.Extensions;
using Deed.Application.Capitals.Queries.GetAll;
using Deed.Application.Capitals.Requests;
using MediatR;
using static System.Net.WebRequestMethods;

namespace Deed.Api.Endpoints.Capitals;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {// TODO add filters
        app.MapGet("api/capitals", async (HttpContext http, ISender sender, string? searchTerm, string? sortBy, string? sortDirection, string? filterBy, CancellationToken ct) =>
            (await sender
                .Send(new GetAllCapitalsQuery(searchTerm, sortBy, sortDirection, filterBy), ct))
                .Process()
            )
            .WithTags(nameof(Capitals))
            .RequireAuthorization();
    }
}
