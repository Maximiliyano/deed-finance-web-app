using Deed.Api.Extensions;
using Deed.Application.BankConnections.Commands.DisconnectPrivat;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class DisconnectPrivat : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/bank-connections/privat", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new DisconnectPrivatCommand(), ct))
                .Process(ResultType.NoContent))
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
