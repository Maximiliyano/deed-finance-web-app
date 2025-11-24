using Deed.Api.Extensions;
using Deed.Application.Categories.Commands.Create;
using Deed.Application.Categories.Requests;
using MediatR;

namespace Deed.Api.Endpoints.Categories;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/categories", async (CreateCategoryRequest request, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new CreateCategoryCommand(
                    request.Name,
                    request.Type,
                    request.PlannedPeriodAmount,
                    request.Period), ct))
                .Process())
            .WithTags(nameof(Categories));
    }
}
