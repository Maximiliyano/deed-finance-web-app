using Deed.Api.Extensions;
using Deed.Application.BudgetEstimations.Commands.UpdateOrders;
using Deed.Application.BudgetEstimations.Requests;
using MediatR;

namespace Deed.Api.Endpoints.BudgetEstimations;

internal sealed class UpdateOrder : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/budget-estimations/orders", async (UpdateEstimationOrdersRequest request, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new UpdateEstimationOrdersCommand(request.Estimations), ct))
                .Process())
            .AllowAnonymous()
            .WithTags(nameof(BudgetEstimations));
    }
}
