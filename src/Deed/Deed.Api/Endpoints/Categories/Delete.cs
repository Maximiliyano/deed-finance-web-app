using Deed.Domain.Results;
using Deed.Api.Extensions;
using Deed.Application.Categories.Commands.Delete;
using MediatR;

namespace Deed.Api.Endpoints.Categories;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/categories/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new DeleteCategoryCommand(id), ct))
                .Process(ResultType.NoContent))
            .WithTags(nameof(Categories));
    }
}
