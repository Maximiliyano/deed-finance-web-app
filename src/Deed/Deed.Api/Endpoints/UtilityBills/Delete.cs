using Deed.Api.Extensions;
using Deed.Application.UtilityBills.Commands.Delete;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.UtilityBills;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/utility-bills/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteUtilityBillCommand(id), ct))
                .Process(ResultType.NoContent))
            .AllowAnonymous()
            .WithTags(nameof(UtilityBills));
    }
}
