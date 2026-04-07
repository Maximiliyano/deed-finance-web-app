using Deed.Api.Extensions;
using Deed.Application.BudgetEstimations.Commands.Update;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.BudgetEstimations;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/budget-estimations/{id:int}", async (int id, UpdateBudgetEstimationRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateBudgetEstimationCommand(id, request.Description, request.BudgetAmount, request.BudgetCurrency, request.CapitalId), ct))
                .Process(ResultType.NoContent))
            .AllowAnonymous()
            .WithTags(nameof(BudgetEstimations));
    }
}
