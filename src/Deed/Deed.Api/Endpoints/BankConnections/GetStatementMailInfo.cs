using Deed.Api.Extensions;
using Deed.Application.BankConnections.Queries.GetStatementMailInfo;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class GetStatementMailInfo : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/bank-connections/mail-info", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetStatementMailInfoQuery(), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
