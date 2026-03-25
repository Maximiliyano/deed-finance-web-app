using Deed.Application.BudgetEstimations.Responses;
using Deed.Application.Capitals.Responses;
using Deed.Application.Categories.Response;
using Deed.Application.Debts.Responses;
using Deed.Application.Exchanges.Responses;
using Deed.Application.Expenses.Responses;
using Deed.Application.Goals.Responses;
using Deed.Application.Incomes.Responses;
using Deed.Application.Transfers.Responses;
using Deed.Application.UserSettings.Responses;

namespace Deed.Application.Dashboard;

public sealed record DashboardResponse(
    UserSettingsResponse? Settings,
    IEnumerable<CapitalResponse> Capitals,
    IEnumerable<ExchangeResponse> Exchanges,
    IEnumerable<BudgetEstimationResponse> Estimations,
    IEnumerable<GoalResponse> Goals,
    IEnumerable<DebtResponse> Debts,
    IEnumerable<CategoryExpenseResponse> ExpenseCategories,
    IEnumerable<IncomeResponse> Incomes,
    IEnumerable<CategoryResponse> IncomeCategories,
    IEnumerable<CategoryResponse> ExpenseCategoriesList,
    IEnumerable<TransferResponse> Transfers);
