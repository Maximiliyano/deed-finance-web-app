namespace Deed.Application.BudgetEstimations.Requests;

public sealed record CreateBudgetEstimationRequest(
    string Description,
    decimal BudgetAmount,
    string BudgetCurrency,
    int? CapitalId
);
