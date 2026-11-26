using Deed.Api.Extensions;
using Deed.Application.BankNotifications.Commands.MarkRead;
using Deed.Application.BankNotifications.Requests;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.BankNotifications;

internal sealed class MarkRead : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bank-notifications/read", async (MarkBankNotificationsReadRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new MarkBankNotificationsReadCommand(request.Ids), ct))
                .Process(ResultType.NoContent))
            .RequireAuthorization()
            .WithTags(nameof(BankNotifications));
    }
}
