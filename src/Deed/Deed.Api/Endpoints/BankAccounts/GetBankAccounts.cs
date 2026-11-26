using Deed.Api.Extensions;
using Deed.Application.BankConnections.Queries.GetBankAccounts;
using MediatR;

namespace Deed.Api.Endpoints.BankAccounts;

internal sealed class GetBankAccounts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/bank-accounts", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetBankAccountsQuery(), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(BankAccounts));
    }
}
