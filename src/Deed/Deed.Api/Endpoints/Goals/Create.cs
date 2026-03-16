using Deed.Api.Extensions;
using Deed.Application.Goals.Commands.Create;
using Deed.Domain.Enums;
using MediatR;

namespace Deed.Api.Endpoints.Goals;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/goals", async (CreateGoalRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CreateGoalCommand(request.Title, request.TargetAmount, request.Currency, request.CurrentAmount, request.Deadline, request.Note), ct))
                .Process())
            .RequireAuthorization()
            .WithTags(nameof(Goals));
    }
}

internal sealed record CreateGoalRequest(
    string Title,
    decimal TargetAmount,
    CurrencyType Currency,
    decimal CurrentAmount,
    DateTimeOffset? Deadline,
    string? Note);
