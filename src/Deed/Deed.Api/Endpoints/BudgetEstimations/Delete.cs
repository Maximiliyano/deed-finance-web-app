using Deed.Api.Extensions;
using Deed.Application.BudgetEstimations.Commands.Delete;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.BudgetEstimations;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/budget-estimations/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteBudgetEstimationCommand(id), ct))
                .Process(ResultType.NoContent))
            .AllowAnonymous()
            .WithTags(nameof(BudgetEstimations));
    }
}
