using Deed.Api.Extensions;
using Deed.Application.BudgetEstimations.Commands.Create;
using Deed.Application.BudgetEstimations.Requests;
using MediatR;

namespace Deed.Api.Endpoints.BudgetEstimations;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/budget-estimations", async (CreateBudgetEstimationRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CreateBudgetEstimationCommand(request.Description, request.BudgetAmount, request.BudgetCurrency, request.CapitalId), ct))
                .Process())
            .AllowAnonymous()
            .WithTags(nameof(BudgetEstimations));
    }
}
