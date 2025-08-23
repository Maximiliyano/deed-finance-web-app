
using Deed.Api.Extensions;
using Deed.Application.Capitals.Commands.UpdateOrders;
using Deed.Application.Capitals.Requests;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.Capitals;

internal sealed class UpdateOrder : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/capitals/orders", async (UpdateCapitalOrdersRequest request, ISender sender) =>
            (await sender
                .Send(new UpdateCapitalOrdersCommand(request.Capitals)))
                .Process())
            .WithTags(nameof(Capitals));
    }
}
