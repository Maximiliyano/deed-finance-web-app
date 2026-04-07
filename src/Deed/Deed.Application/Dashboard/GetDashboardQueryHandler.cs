using Deed.Application.Abstractions.Caching;
using Deed.Application.Abstractions.Messaging;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Auth;
using Deed.Application.BudgetEstimations;
using Deed.Application.BudgetEstimations.Specifications;
using Deed.Application.Capitals;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Categories;
using Deed.Application.Categories.Specifications;
using Deed.Application.Debts;
using Deed.Application.Debts.Specifications;
using Deed.Application.Exchanges;
using Deed.Application.Exchanges.Responses;
using Deed.Application.Exchanges.Specifications;
using Deed.Application.Expenses;
using Deed.Application.Expenses.Responses;
using Deed.Application.Expenses.Specifications;
using Deed.Application.Goals;
using Deed.Application.Goals.Specifications;
using Deed.Application.Incomes;
using Deed.Application.Incomes.Specifications;
using Deed.Application.Transfers.Responses;
using Deed.Application.Transfers.Specifications;
using Deed.Application.UserSettings;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Microsoft.Extensions.Options;

namespace Deed.Application.Dashboard;

internal sealed class GetDashboardQueryHandler(
    IUser user,
    ICapitalRepository capitalRepository,
    IExchangeRepository exchangeRepository,
    IBudgetEstimationRepository estimationRepository,
    IGoalRepository goalRepository,
    IDebtRepository debtRepository,
    IExpenseRepository expenseRepository,
    IIncomeRepository incomeRepository,
    ICategoryRepository categoryRepository,
    IUserSettingsRepository userSettingsRepository,
    ITransferRepository transferRepository,
    ICacheService cacheService,
    IOptions<MemoryCacheSettings> cacheSettings)
    : IQueryHandler<GetDashboardQuery, DashboardResponse>
{
    public async Task<Result<DashboardResponse>> Handle(GetDashboardQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(user.Name);

        Domain.Entities.UserSettings? settings =
            await userSettingsRepository.GetAsync(user.Name, ct).ConfigureAwait(false);
        IEnumerable<Capital> capitals = await capitalRepository
            .GetAllAsync(new CapitalsByQueryParamsSpecification(user.Name, disableIncludes: true), ct).ConfigureAwait(false);
        IEnumerable<BudgetEstimation> estimations = await estimationRepository
            .GetAllAsync(new BudgetEstimationsByUserSpecification(user.Name, true), ct).ConfigureAwait(false);
        IEnumerable<Goal> goals = await goalRepository.GetAllAsync(new GoalsByUserSpecification(user.Name), ct)
            .ConfigureAwait(false);
        IEnumerable<Debt> debts = await debtRepository.GetAllAsync(new DebtsByUserSpecification(user.Name), ct)
            .ConfigureAwait(false);
        IEnumerable<Expense> expenseList = await expenseRepository
            .GetAllAsync(new ExpenseByQueriesSpecification(user.Name), ct).ConfigureAwait(false);
        IEnumerable<Income> incomeList = await incomeRepository
            .GetAllAsync(new IncomesByQuerySpecification(user.Name), ct).ConfigureAwait(false);
        IEnumerable<Category> allCategories = await categoryRepository
            .GetAllAsync(new CategoriesByQuerySpecification([]), ct).ConfigureAwait(false);
        IEnumerable<Category> expenseCats = allCategories.Where(c => c.Type == CategoryType.Expenses);
        IEnumerable<Category> incomeCats = allCategories.Where(c => c.Type == CategoryType.Incomes);
        IEnumerable<Transfer> transferList = await transferRepository
            .GetAllAsync(new TransfersByUserSpecification(user.Name), ct).ConfigureAwait(false);

        List<ExchangeResponse>? cached = await cacheService.GetAsync<List<ExchangeResponse>>(CacheKeys.Exchanges, ct).ConfigureAwait(false);
        if (cached is null)
        {
            cached = (await exchangeRepository.GetAllAsync(new ExchangesByQuerySpecification(), ct).ConfigureAwait(false)).ToResponses().ToList();
            if (cached.Count > 0)
            {
                await cacheService.SetAsync(CacheKeys.Exchanges, cached, TimeSpan.FromHours(cacheSettings.Value.ExchangesTimespanInHours), ct).ConfigureAwait(false);
            }
        }
        IEnumerable<ExchangeResponse> exchanges = cached;

        decimal totalSum = expenseList.Sum(e => e.Amount);
        const decimal epsilon = 0.0001m;

        IEnumerable<CategoryExpenseResponse> expenseCategories = expenseList
            .GroupBy(e => e.CategoryId)
            .Select(g =>
            {
                decimal categorySum = g.Sum(e => e.Amount);
                Category? category = g.First().Category;
                decimal percentage = Math.Abs(totalSum) < epsilon ? 0m : Math.Round(categorySum / totalSum * 100, 2);

                return new CategoryExpenseResponse(
                    g.Key,
                    category?.Name ?? "Undefined",
                    categorySum,
                    percentage,
                    category?.PlannedPeriodAmount ?? 0m,
                    category?.Period.ToString() ?? nameof(PerPeriodType.None),
                    g.ToResponses());
            });

        IEnumerable<TransferResponse> transfers = transferList.Select(t => new TransferResponse(
            t.Id, t.Amount, t.DestinationAmount,
            t.SourceCapitalId, t.SourceCapital?.Name, t.SourceCapital?.Currency.ToString(),
            t.DestinationCapitalId, t.DestinationCapital?.Name, t.DestinationCapital?.Currency.ToString(),
            t.CreatedAt));

        DashboardResponse response = new(
            settings?.ToResponse(),
            capitals.ToResponses(),
            exchanges,
            estimations.ToResponses(),
            goals.ToResponses(),
            debts.ToResponses(),
            expenseCategories,
            incomeList.ToResponses(),
            incomeCats.ToResponses(),
            expenseCats.ToResponses(),
            transfers);

        return Result.Success(response);
    }

}
