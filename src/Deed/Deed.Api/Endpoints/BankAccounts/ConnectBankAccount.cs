using Deed.Api.Extensions;
using Deed.Application.BankConnections.Commands.ConnectBankAccount;
using Deed.Application.BankConnections.Requests;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.BankAccounts;

internal sealed class ConnectBankAccount : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bank-accounts/{id:int}/connect", async (int id, ConnectBankAccountRequest? request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ConnectBankAccountCommand(id, request?.Label), ct))
                .Process(ResultType.NoContent))
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicies.Bank)
            .WithTags(nameof(BankAccounts));
    }
}
