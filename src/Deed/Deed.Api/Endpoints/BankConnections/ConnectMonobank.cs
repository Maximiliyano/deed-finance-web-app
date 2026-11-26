using Deed.Api.Extensions;
using Deed.Application.BankConnections.Commands.ConnectMonobank;
using Deed.Application.BankConnections.Requests;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class ConnectMonobank : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bank-connections/monobank", async (ConnectMonobankRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ConnectMonobankCommand(request.Token, request.AccountId, request.Iban, request.Label), ct))
                .Process(ResultType.NoContent))
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
