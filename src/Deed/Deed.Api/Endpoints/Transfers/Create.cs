using Deed.Api.Extensions;
using Deed.Application.Transfers.Commands.Create;
using MediatR;

namespace Deed.Api.Endpoints.Transfers;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/transfers", async (CreateTransferRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CreateTransferCommand(
                request.SourceCapitalId,
                request.DestinationCapitalId,
                request.Amount,
                request.DestinationAmount), ct))
                .Process())
            .AllowAnonymous()
            .WithTags(nameof(Transfers));
    }
}

internal sealed record CreateTransferRequest(
    int SourceCapitalId,
    int DestinationCapitalId,
    decimal Amount,
    decimal DestinationAmount);
