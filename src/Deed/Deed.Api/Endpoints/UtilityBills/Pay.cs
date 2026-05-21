using Deed.Api.Extensions;
using Deed.Application.UtilityBills.Commands.Pay;
using MediatR;

namespace Deed.Api.Endpoints.UtilityBills;

internal sealed class Pay : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/utility-bills/{id:int}/pay", async (int id, PayUtilityBillRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new PayUtilityBillCommand(id, request.Amount, request.PaymentDate), ct))
                .Process())
            .AllowAnonymous()
            .WithTags(nameof(UtilityBills));
    }
}
