using Deed.Api.Extensions;
using Deed.Application.BankConnections.Commands.DisconnectBankConnection;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class DisconnectBankConnection : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/bank-connections/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DisconnectBankConnectionCommand(id), ct))
                .Process(ResultType.NoContent))
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
