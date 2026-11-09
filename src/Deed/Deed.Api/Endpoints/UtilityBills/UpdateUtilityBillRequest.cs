namespace Deed.Api.Endpoints.UtilityBills;

internal sealed record UpdateUtilityBillRequest(
    string Name,
    decimal EstimatedAmount,
    string Currency,
    int DueDayOfMonth,
    int CapitalId,
    int CategoryId,
    bool IsActive);
