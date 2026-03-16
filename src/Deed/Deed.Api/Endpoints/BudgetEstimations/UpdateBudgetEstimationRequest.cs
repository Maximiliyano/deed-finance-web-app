using Deed.Domain.Enums;

namespace Deed.Api.Endpoints.BudgetEstimations;

internal sealed record UpdateBudgetEstimationRequest(
    string Description,
    decimal BudgetAmount,
    CurrencyType BudgetCurrency,
    int? CapitalId);
