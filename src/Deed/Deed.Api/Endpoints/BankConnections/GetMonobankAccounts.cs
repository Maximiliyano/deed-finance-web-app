using Deed.Api.Extensions;
using Deed.Application.BankConnections.Queries.GetMonobankAccounts;
using Deed.Application.BankConnections.Requests;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class GetMonobankAccounts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bank-connections/monobank/accounts", async (MonobankAccountsRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetMonobankAccountsQuery(request.Token), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
