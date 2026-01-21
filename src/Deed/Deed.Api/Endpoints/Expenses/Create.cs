using Deed.Api.Extensions;
using Deed.Application.Expenses.Commands.Create;
using Deed.Application.Expenses.Requests;
using MediatR;

namespace Deed.Api.Endpoints.Expenses;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/expenses", async (CreateExpenseRequest request, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new CreateExpenseCommand(
                    request.CapitalId,
                    request.CategoryId,
                    request.Amount,
                    request.PaymentDate,
                    request.Purpose,
                    request.TagNames), ct))
                .Process())
            .RequireAuthorization()
            .WithTags(nameof(Expenses));
    }
}
