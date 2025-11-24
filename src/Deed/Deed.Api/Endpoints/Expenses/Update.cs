using Deed.Api.Extensions;
using Deed.Application.Expenses.Commands.Update;
using Deed.Application.Expenses.Requests;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.Expenses;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/expenses", async (UpdateExpenseRequest request, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new UpdateExpenseCommand(
                    request.Id,
                    request.CategoryId,
                    request.CapitalId,
                    request.Amount,
                    request.Purpose,
                    request.Date), ct))
                .Process(ResultType.NoContent))
            .WithTags(nameof(Expenses));
    }
}
