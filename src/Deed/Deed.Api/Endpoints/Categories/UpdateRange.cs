using Deed.Api.Extensions;
using Deed.Application.Categories.Commands.UpdateRange;
using Deed.Application.Categories.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Deed.Api.Endpoints.Categories;

internal sealed class UpdateRange : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/categories/updateRange", async (
            [FromBody] IEnumerable<UpdateCategoryRequest> requests,
            ISender sender,
            CancellationToken ct
        ) =>
            (await sender
                .Send(new UpdateCategoriesCommand(requests), ct))
                .Process())
            .WithTags(nameof(Categories));
    }
}
