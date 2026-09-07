namespace Deed.Application.BudgetEstimations.Requests;

public sealed record GetAllBudgetEstimationRequest(
    DateTime PeriodStart,
    DateTime PeriodEnd
);
