using Deed.Api.Extensions;
using Deed.Application.BankConnections.Commands.RefreshBankAccounts;
using MediatR;

namespace Deed.Api.Endpoints.BankAccounts;

internal sealed class RefreshBankAccounts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bank-accounts/refresh", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new RefreshBankAccountsCommand(), ct)).Process())
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicies.Bank)
            .WithTags(nameof(BankAccounts));
    }
}
