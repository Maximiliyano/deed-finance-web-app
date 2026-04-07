using Deed.Api.Extensions;
using Deed.Application.Debts.Commands.UpdateOrders;
using Deed.Application.Debts.Requests;
using MediatR;

namespace Deed.Api.Endpoints.Debts;

internal sealed class UpdateOrder : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/debts/orders", async (UpdateDebtOrdersRequest request, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new UpdateDebtOrdersCommand(request.Debts), ct))
                .Process())
            .AllowAnonymous()
            .WithTags(nameof(Debts));
    }
}
