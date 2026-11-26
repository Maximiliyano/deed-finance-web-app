using Deed.Api.Extensions;
using Deed.Application.BankNotifications.Commands.Import;
using Deed.Application.BankNotifications.Requests;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.BankNotifications;

internal sealed class Import : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bank-notifications/{id:int}/import", async (int id, ImportBankNotificationRequest? request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ImportBankNotificationCommand(id, request?.CategoryId), ct))
                .Process(ResultType.NoContent))
            .RequireAuthorization()
            .WithTags(nameof(BankNotifications));
    }
}
