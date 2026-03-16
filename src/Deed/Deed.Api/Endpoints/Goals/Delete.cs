using Deed.Api.Extensions;
using Deed.Application.Goals.Commands.Delete;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.Goals;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/goals/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteGoalCommand(id), ct))
                .Process(ResultType.NoContent))
            .RequireAuthorization()
            .WithTags(nameof(Goals));
    }
}
