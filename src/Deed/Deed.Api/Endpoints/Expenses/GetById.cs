using Deed.Api.Extensions;
using Deed.Application.Expenses.Queries.GetById;
using MediatR;

namespace Deed.Api.Endpoints.Expenses;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/expenses/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new GetExpenseByIdQuery(id), ct))
                .Process())
            .RequireAuthorization()
            .WithTags(nameof(Expenses));
    }
}
