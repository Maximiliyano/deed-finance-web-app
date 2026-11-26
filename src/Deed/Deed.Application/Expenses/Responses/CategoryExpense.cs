namespace Deed.Application.Expenses.Responses;

public sealed record CategoryExpenseResponse(
    int CategoryId,
    string Name,
    decimal CategorySum,
    decimal Percentage,
    decimal PlannedPeriodAmount,
    string PeriodType,
    IEnumerable<ExpenseResponse> Expenses
);
