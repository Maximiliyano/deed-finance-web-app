using Deed.Domain.Enums;

namespace Deed.Api.Endpoints.BudgetEstimations;

internal sealed record CreateBudgetEstimationRequest(
    string Description,
    decimal BudgetAmount,
    CurrencyType BudgetCurrency,
    int? CapitalId);
