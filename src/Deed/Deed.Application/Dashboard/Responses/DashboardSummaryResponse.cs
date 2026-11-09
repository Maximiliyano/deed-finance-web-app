namespace Deed.Application.Dashboard.Responses;

public sealed record DashboardSummaryResponse(
    string Currency,
    decimal TotalCapital,
    decimal TotalCapitalInDefaultCurrency,
    decimal TotalIncome,
    decimal TotalIncomeInDefaultCurrency,
    decimal TotalExpense,
    decimal TotalExpenseInDefaultCurrency,
    decimal TotalBudget,
    decimal TotalBudgetInDefaultCurrency,
    IReadOnlyList<CategoryAmountResponse> IncomesByCategory,
    IReadOnlyList<CategoryAmountResponse> ExpensesByCategory);

public sealed record CategoryAmountResponse(int CategoryId, string Name, decimal Amount, decimal Percentage);
