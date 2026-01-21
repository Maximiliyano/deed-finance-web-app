using Deed.Api.Extensions;
using Deed.Application.Incomes.Queries.GetById;
using MediatR;

namespace Deed.Api.Endpoints.Incomes;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/incomes/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new GetIncomeByIdQuery(id), ct))
                .Process())
            .RequireAuthorization()
            .WithTags(nameof(Incomes));
    }
}
