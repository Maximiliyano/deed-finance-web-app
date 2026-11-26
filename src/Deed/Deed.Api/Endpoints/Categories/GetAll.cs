using Deed.Api.Extensions;
using Deed.Application.Categories.Queries.GetAll;
using Deed.Domain.Enums;
using MediatR;

namespace Deed.Api.Endpoints.Categories;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/categories", async (
                CategoryType? type,
                bool? includeDeleted,
                ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new GetAllCategoryQuery(type, includeDeleted), ct))
                .Process())
            .AllowAnonymous()
            .WithTags(nameof(Categories));
    }
}
