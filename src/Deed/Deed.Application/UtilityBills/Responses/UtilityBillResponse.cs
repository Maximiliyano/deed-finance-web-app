namespace Deed.Application.UtilityBills.Responses;

public sealed record UtilityBillResponse(
    int Id,
    string Name,
    decimal EstimatedAmount,
    string Currency,
    int DueDayOfMonth,
    int CapitalId,
    string? CapitalName,
    int CategoryId,
    string? CategoryName,
    bool IsActive,
    bool IsPaidThisMonth,
    decimal? PaidAmount,
    DateTimeOffset? PaidAt,
    int? PaidExpenseId
);
