using Deed.Api.Extensions;
using Deed.Application.BankNotifications.Commands.Dismiss;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.BankNotifications;

internal sealed class Dismiss : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bank-notifications/{id:int}/dismiss", async (int id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DismissBankNotificationCommand(id), ct))
                .Process(ResultType.NoContent))
            .RequireAuthorization()
            .WithTags(nameof(BankNotifications));
    }
}
