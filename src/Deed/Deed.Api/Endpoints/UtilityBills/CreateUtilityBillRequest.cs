using Deed.Domain.Enums;

namespace Deed.Api.Endpoints.UtilityBills;

internal sealed record CreateUtilityBillRequest(
    string Name,
    decimal EstimatedAmount,
    string Currency,
    int DueDayOfMonth,
    int CapitalId,
    int CategoryId,
    bool IsActive);
