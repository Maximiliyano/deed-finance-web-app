using Deed.Api.Extensions;
using Deed.Application.BankConnections.Commands.SyncBankConnections;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class Sync : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bank-connections/sync", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new SyncBankConnectionsCommand(), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
