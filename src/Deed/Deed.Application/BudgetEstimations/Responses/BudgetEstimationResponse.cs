namespace Deed.Application.BudgetEstimations.Responses;

public sealed record BudgetEstimationResponse(
    int Id,
    string Description,
    decimal BudgetAmount,
    string BudgetCurrency,
    int? CapitalId,
    string? CapitalName,
    decimal CapitalBalance,
    int OrderIndex,
    bool IsCompleted
);
