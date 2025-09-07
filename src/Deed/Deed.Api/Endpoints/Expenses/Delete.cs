using Deed.Domain.Results;
using Deed.Api.Extensions;
using Deed.Application.Expenses.Commands.Delete;
using MediatR;

namespace Deed.Api.Endpoints.Expenses;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/expenses/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new DeleteExpenseCommand(id), ct))
                .Process(ResultType.NoContent))
            .WithTags(nameof(Expenses));
    }
}
