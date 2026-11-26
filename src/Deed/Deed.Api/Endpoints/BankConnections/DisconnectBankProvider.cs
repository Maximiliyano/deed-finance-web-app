using Deed.Api.Extensions;
using Deed.Application.BankConnections.Commands.DisconnectBankProvider;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class DisconnectBankProvider : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/bank-connections/provider/{provider}", async (string provider, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DisconnectBankProviderCommand(provider), ct))
                .Process(ResultType.NoContent))
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
