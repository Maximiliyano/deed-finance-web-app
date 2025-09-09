namespace Deed.Application.Capitals.Responses;

public sealed record CapitalResponse(
    int Id,
    string Name,
    float Balance,
    string Currency,
    bool IncludeInTotal,
    bool OnlyForSavings,
    float TotalIncome,
    float TotalExpense,
    float TotalTransferIn,
    float TotalTransferOut);
