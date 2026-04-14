using Deed.Api.Extensions;
using Deed.Application.Goals.Commands.UpdateOrders;
using Deed.Application.Goals.Requests;
using MediatR;

namespace Deed.Api.Endpoints.Goals;

internal sealed class UpdateOrder : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/goals/orders", async (UpdateGoalOrdersRequest request, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new UpdateGoalOrdersCommand(request.Goals), ct))
                .Process())
            .AllowAnonymous()
            .WithTags(nameof(Goals));
    }
}
