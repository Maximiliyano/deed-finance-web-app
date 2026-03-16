using Deed.Api.Extensions;
using Deed.Application.Transfers.Commands.Delete;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.Transfers;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/transfers/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteTransferCommand(id), ct))
                .Process(ResultType.NoContent))
            .RequireAuthorization()
            .WithTags(nameof(Transfers));
    }
}
