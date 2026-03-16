using Deed.Api.Extensions;
using Deed.Application.Transfers.Queries.GetAll;
using MediatR;

namespace Deed.Api.Endpoints.Transfers;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/transfers", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetAllTransfersQuery(), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(Transfers));
    }
}
