
using Deed.Api.Extensions;
using Deed.Application.Capitals.Commands.UpdateOrders;
using Deed.Application.Capitals.Requests;
using MediatR;

namespace Deed.Api.Endpoints.Capitals;

internal sealed class UpdateOrder : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/capitals/orders", async (UpdateCapitalOrdersRequest request, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new UpdateCapitalOrdersCommand(request.Capitals), ct))
                .Process())
            .RequireAuthorization()
            .WithTags(nameof(Capitals));
    }
}
