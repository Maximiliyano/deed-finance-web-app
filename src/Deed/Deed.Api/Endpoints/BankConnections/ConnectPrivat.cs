using Deed.Api.Extensions;
using Deed.Application.BankConnections.Commands.ConnectPrivat;
using Deed.Application.BankConnections.Requests;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class ConnectPrivat : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bank-connections/privat", async (ConnectPrivatRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ConnectPrivatCommand(request.Token, request.Iban), ct))
                .Process(ResultType.NoContent))
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
