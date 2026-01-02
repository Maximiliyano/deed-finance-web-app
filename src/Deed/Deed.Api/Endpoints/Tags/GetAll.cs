using Deed.Api.Extensions;
using Deed.Application.Tags.Queries.GetAll;
using MediatR;

namespace Deed.Api.Endpoints.Tags;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/tags", async (string? term, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new GetAllExpenseTagsQuery(term), ct))
                .Process())
            .RequireAuthorization()
            .WithTags(nameof(Tags));
    }
}
