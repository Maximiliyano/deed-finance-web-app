using Deed.Api.Extensions;
using Deed.Application.Abstractions.Data;
using Deed.Application.Capitals.Queries.GetAll;
using MediatR;

namespace Deed.Api.Endpoints.Capitals;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/capitals/all", async (QueryParams query, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new GetAllCapitalsQuery(query.SearchTerm, query.SortBy, query.SortDirection, query.FilterBy), ct))
                .Process()
            )
            .RequireAuthorization()
            .WithTags(nameof(Capitals));
    }
}
