using Deed.Api.Extensions;
using Deed.Application.BankConnections.Queries.GetAll;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/bank-connections", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetBankConnectionsQuery(), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
