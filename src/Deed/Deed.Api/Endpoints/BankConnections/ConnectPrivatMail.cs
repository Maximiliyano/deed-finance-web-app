using Deed.Api.Extensions;
using Deed.Application.BankConnections.Commands.ConnectPrivatMail;
using Deed.Application.BankConnections.Requests;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class ConnectPrivatMail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bank-connections/privat-mail", async (ConnectPrivatMailRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ConnectPrivatMailCommand(request.Iban, request.Label, request.Currency), ct)).Process())
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicies.Bank)
            .WithTags(nameof(BankConnections));
    }
}
