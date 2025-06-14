using Deed.Domain.Enums;
using Deed.Api.Extensions;
using Deed.Application.Categories.Queries.GetAll;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Deed.Api.Endpoints.Categories;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/categories", async (ISender sender, [FromQuery] CategoryType? type) =>
            (await sender
                .Send(new GetAllCategoryQuery(type)))
                .Process())
            .WithTags(nameof(Categories));
    }
}
