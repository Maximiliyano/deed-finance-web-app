using Deed.Api.Extensions;
using Deed.Application.Categories.Commands.UpdateRange;
using Deed.Application.Categories.Requests;
using MediatR;

namespace Deed.Api.Endpoints.Categories;

internal sealed class UpdateRange : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/categories/updateRange", async (ISender sender, IEnumerable<UpdateCategoryRequest> requests, CancellationToken ct) =>
            (await sender
                .Send(new UpdateCategoriesCommand(requests), ct))
                .Process())
            .WithTags(nameof(Categories));
    }
}
