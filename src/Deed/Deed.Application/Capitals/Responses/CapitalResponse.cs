namespace Deed.Application.Capitals.Responses;

public sealed record CapitalResponse(
    int Id,
    string Name,
    decimal Balance,
    string Currency,
    bool IncludeInTotal,
    bool OnlyForSavings,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal TotalTransferIn,
    decimal TotalTransferOut,
    DateTimeOffset CreatedAt,
    string? CreatedBy
);
