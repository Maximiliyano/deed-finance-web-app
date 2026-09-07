using Deed.Api.Extensions;
using Deed.Application.BudgetEstimations.Queries.GetAll;
using Deed.Application.BudgetEstimations.Requests;
using MediatR;

namespace Deed.Api.Endpoints.BudgetEstimations;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/budget-estimations/all", async (GetAllBudgetEstimationRequest request, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new GetAllBudgetEstimationsQuery(request.PeriodStart, request.PeriodEnd), ct))
                .Process())
            .AllowAnonymous()
            .WithTags(nameof(BudgetEstimations));
    }
}
