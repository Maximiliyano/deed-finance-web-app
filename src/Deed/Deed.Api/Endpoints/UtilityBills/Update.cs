using Deed.Api.Extensions;
using Deed.Application.UtilityBills.Commands.Update;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.UtilityBills;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/utility-bills/{id:int}", async (int id, UpdateUtilityBillRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateUtilityBillCommand(
                id,
                request.Name,
                request.EstimatedAmount,
                request.Currency,
                request.DueDayOfMonth,
                request.CapitalId,
                request.CategoryId,
                request.IsActive), ct))
                .Process(ResultType.NoContent))
            .AllowAnonymous()
            .WithTags(nameof(UtilityBills));
    }
}
