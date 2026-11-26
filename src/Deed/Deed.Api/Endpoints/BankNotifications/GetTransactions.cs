using Deed.Api.Extensions;
using Deed.Application.BankNotifications.Queries.GetPage;
using MediatR;

namespace Deed.Api.Endpoints.BankNotifications;

internal sealed class GetTransactions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/bank-transactions", async (int? page, int? pageSize, string? type, string? status, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetBankTransactionsQuery(page ?? 1, pageSize ?? 20, type, status), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(BankNotifications));
    }
}
