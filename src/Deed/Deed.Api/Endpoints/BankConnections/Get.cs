using Deed.Api.Extensions;
using Deed.Application.BankConnections.Queries.Get;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/bank-connections", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetBankConnectionQuery(), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
