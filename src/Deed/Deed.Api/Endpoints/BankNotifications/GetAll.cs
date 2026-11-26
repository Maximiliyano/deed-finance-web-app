using Deed.Api.Extensions;
using Deed.Application.BankNotifications.Queries.GetAll;
using MediatR;

namespace Deed.Api.Endpoints.BankNotifications;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/bank-notifications", async (bool? unreadOnly, int? limit, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetBankNotificationsQuery(unreadOnly ?? false, limit ?? 30), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(BankNotifications));
    }
}
