using Deed.Api.Extensions;
using Deed.Application.BankNotifications.Queries.GetSummary;
using MediatR;

namespace Deed.Api.Endpoints.BankNotifications;

internal sealed class GetTransactionsSummary : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/bank-transactions/summary", async (string? period, int? connectionId, string? provider, DateTimeOffset? from, DateTimeOffset? to, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetBankTransactionsSummaryQuery(period, connectionId, from, to, provider), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(BankNotifications));
    }
}
