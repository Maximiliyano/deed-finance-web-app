using Deed.Domain.Enums;

namespace Deed.Api.Endpoints.BudgetEstimations;

public sealed record UpdateBudgetEstimationRequest(
    string Description,
    decimal BudgetAmount,
    CurrencyType BudgetCurrency,
    int? CapitalId,
    bool IsCompleted
);
