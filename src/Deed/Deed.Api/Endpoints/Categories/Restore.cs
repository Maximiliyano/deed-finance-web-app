
using Deed.Api.Extensions;
using Deed.Application.Categories.Commands.Restore;
using MediatR;

namespace Deed.Api.Endpoints.Categories;

internal sealed class Restore : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/categories/{id:int}/restore", async (int id, ISender sender, CancellationToken cancellationToken) =>
            (await sender
                .Send(new RestoreCategoryCommand(id), cancellationToken))
                .Process())
            .WithTags(nameof(Categories));
    }
}
