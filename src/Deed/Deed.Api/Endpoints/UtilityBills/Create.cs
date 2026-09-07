using Deed.Api.Extensions;
using Deed.Application.UtilityBills.Commands.Create;
using MediatR;

namespace Deed.Api.Endpoints.UtilityBills;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/utility-bills", async (CreateUtilityBillRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CreateUtilityBillCommand(
                request.Name,
                request.EstimatedAmount,
                request.Currency,
                request.DueDayOfMonth,
                request.CapitalId,
                request.CategoryId,
                request.IsActive), ct))
                .Process())
            .AllowAnonymous()
            .WithTags(nameof(UtilityBills));
    }
}
