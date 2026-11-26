using Deed.Api.Extensions;
using Deed.Application.BankConnections.Commands.ConnectPrivatCard;
using Deed.Application.BankConnections.Requests;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class ConnectPrivatCard : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bank-connections/privat-card", async (ConnectPrivatCardRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ConnectPrivatCardCommand(request.MerchantId, request.Password, request.CardNumber), ct))
                .Process(ResultType.NoContent))
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
