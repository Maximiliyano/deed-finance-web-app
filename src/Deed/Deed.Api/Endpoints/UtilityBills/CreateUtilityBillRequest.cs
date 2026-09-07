using Deed.Domain.Enums;

namespace Deed.Api.Endpoints.UtilityBills;

internal sealed record CreateUtilityBillRequest(
    string Name,
    decimal EstimatedAmount,
    CurrencyType Currency,
    int DueDayOfMonth,
    int CapitalId,
    int CategoryId,
    bool IsActive);
