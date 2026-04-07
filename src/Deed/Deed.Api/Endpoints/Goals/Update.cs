using Deed.Api.Extensions;
using Deed.Application.Goals.Commands.Update;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.Goals;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/goals/{id:int}", async (int id, UpdateGoalRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateGoalCommand(id, request.Title, request.TargetAmount, request.Currency, request.CurrentAmount, request.Deadline, request.Note, request.IsCompleted), ct))
                .Process(ResultType.NoContent))
            .AllowAnonymous()
            .WithTags(nameof(Goals));
    }
}
