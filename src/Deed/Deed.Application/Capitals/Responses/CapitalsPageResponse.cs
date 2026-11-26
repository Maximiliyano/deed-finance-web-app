namespace Deed.Application.Capitals.Responses;

public sealed record CapitalsPageResponse(
    float TotalBalance,
    float TotalExpenses,
    float TotalIncomes,
    float TotalTransfersIn,
    float TotalTransfersOut,
    IEnumerable<CapitalResponse> Capitals
);
