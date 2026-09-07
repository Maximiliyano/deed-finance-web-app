namespace Deed.Api.Endpoints.BudgetEstimations;

internal sealed record UpdateBudgetEstimationRequest(
    string Description,
    decimal BudgetAmount,
    string BudgetCurrency,
    int? CapitalId,
    bool IsCompleted);
