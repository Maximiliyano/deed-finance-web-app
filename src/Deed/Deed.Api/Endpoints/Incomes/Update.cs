using Deed.Api.Extensions;
using Deed.Application.Incomes.Commands.Update;
using Deed.Application.Incomes.Requests;
using MediatR;

namespace Deed.Api.Endpoints.Incomes;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/incomes", async (UpdateIncomeRequest request, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new UpdateIncomeCommand(
                    request.Id,
                    request.CategoryId,
                    request.Amount,
                    request.Purpose,
                    request.PaymentDate), ct))
                .Process())
            .WithTags(nameof(Incomes));
    }
}
