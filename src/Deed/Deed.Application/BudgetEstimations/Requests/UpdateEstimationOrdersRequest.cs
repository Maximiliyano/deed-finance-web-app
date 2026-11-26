namespace Deed.Application.BudgetEstimations.Requests;

public sealed record UpdateEstimationOrdersRequest(
    IEnumerable<EstimationOrder> Estimations
);
