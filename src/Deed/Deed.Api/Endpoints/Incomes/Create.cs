using Deed.Api.Extensions;
using Deed.Application.Incomes.Commands.Create;
using Deed.Application.Incomes.Requests;
using MediatR;

namespace Deed.Api.Endpoints.Incomes;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/incomes", async (CreateIncomeRequest request, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new CreateIncomeCommand(
                    request.CapitalId ?? 0,
                    request.CategoryId ?? 0,
                    request.Amount,
                    request.PaymentDate,
                    request.Purpose,
                    request.Tags ?? []), ct))
                .Process())
            .RequireAuthorization()
            .WithTags(nameof(Incomes));
    }
}
