using Deed.Api.Extensions;
using Deed.Application.Incomes.Queries.GetAll;
using MediatR;

namespace Deed.Api.Endpoints.Incomes;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/incomes", async (ISender sender) =>
            (await sender
                .Send(new GetIncomesQuery()))
                .Process())
            .WithTags(nameof(Incomes));
    }
}
