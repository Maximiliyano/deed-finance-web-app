namespace Deed.Application.UtilityBills.Responses;

public sealed record UtilityBillResponse(
    int Id,
    string Name,
    decimal EstimatedAmount,
    string Currency,
    int DueDayOfMonth,
    int CapitalId,
    string? CapitalName,
    string? CapitalCurrency,
    int CategoryId,
    string? CategoryName,
    bool IsActive,
    bool IsPaidThisMonth,
    decimal? PaidAmount,
    DateTimeOffset? PaidAt,
    DateTimeOffset CreatedAt,
    decimal? UnitPrice = null,
    string? Unit = null,
    decimal? PreviousReading = null,
    decimal? PaidMeterReading = null
);
