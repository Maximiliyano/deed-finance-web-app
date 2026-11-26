using Deed.Api.Extensions;
using Deed.Application.BankConnections.Queries.GetPrivatAccounts;
using Deed.Application.BankConnections.Requests;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class GetPrivatAccounts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bank-connections/privat/accounts", async (PrivatAccountsRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetPrivatAccountsQuery(request.Token), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
