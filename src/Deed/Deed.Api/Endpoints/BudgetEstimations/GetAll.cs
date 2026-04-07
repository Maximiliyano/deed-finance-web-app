using Deed.Api.Extensions;
using Deed.Application.BudgetEstimations.Queries.GetAll;
using MediatR;

namespace Deed.Api.Endpoints.BudgetEstimations;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/budget-estimations", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetAllBudgetEstimationsQuery(), ct)).Process())
            .AllowAnonymous()
            .WithTags(nameof(BudgetEstimations));
    }
}
