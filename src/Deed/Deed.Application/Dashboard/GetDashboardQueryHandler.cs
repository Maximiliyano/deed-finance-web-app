using Deed.Application.Abstractions.Caching;
using Deed.Application.Abstractions.Data;
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
    IDeedDbContextFactory contextFactory,
    IUserSettingsRepository userSettingsRepository,
    ICacheService cacheService,
    IOptions<MemoryCacheSettings> cacheSettings)
    : IQueryHandler<GetDashboardQuery, DashboardResponse>
{
    public async Task<Result<DashboardResponse>> Handle(GetDashboardQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(user.Name);

        Task<Domain.Entities.UserSettings?> settingsTask =
            userSettingsRepository.GetAsync(user.Name, ct);

        using var capitalsCtx = contextFactory.CreateReadOnlyContext();
        using var estimationsCtx = contextFactory.CreateReadOnlyContext();
        using var goalsCtx = contextFactory.CreateReadOnlyContext();
        using var debtsCtx = contextFactory.CreateReadOnlyContext();
        using var expensesCtx = contextFactory.CreateReadOnlyContext();
        using var incomesCtx = contextFactory.CreateReadOnlyContext();
        using var categoriesCtx = contextFactory.CreateReadOnlyContext();
        using var transfersCtx = contextFactory.CreateReadOnlyContext();

        Task<List<Capital>> capitalsTask = capitalsCtx
            .QueryAsync(new CapitalsByQueryParamsSpecification(user.Name, disableIncludes: true), ct);
        Task<List<BudgetEstimation>> estimationsTask = estimationsCtx
            .QueryAsync(new BudgetEstimationsByUserSpecification(user.Name, true), ct);
        Task<List<Goal>> goalsTask = goalsCtx
            .QueryAsync(new GoalsByUserSpecification(user.Name), ct);
        Task<List<Debt>> debtsTask = debtsCtx
            .QueryAsync(new DebtsByUserSpecification(user.Name), ct);
        Task<List<Expense>> expensesTask = expensesCtx
            .QueryAsync(new ExpenseByQueriesSpecification(user.Name), ct);
        Task<List<Income>> incomesTask = incomesCtx
            .QueryAsync(new IncomesByQuerySpecification(user.Name), ct);
        Task<List<Category>> categoriesTask = categoriesCtx
            .QueryAsync(new CategoriesByQuerySpecification([]), ct);
        Task<List<Transfer>> transfersTask = transfersCtx
            .QueryAsync(new TransfersByUserSpecification(user.Name), ct);

        Task<List<ExchangeResponse>?> exchangesCacheTask =
            cacheService.GetAsync<List<ExchangeResponse>>(CacheKeys.Exchanges, ct);

        await Task.WhenAll(
            settingsTask, capitalsTask, estimationsTask, goalsTask, debtsTask,
            expensesTask, incomesTask, categoriesTask, transfersTask, exchangesCacheTask).ConfigureAwait(false);

        Domain.Entities.UserSettings? settings = await settingsTask.ConfigureAwait(false);
        List<Capital> capitals = await capitalsTask.ConfigureAwait(false);
        List<BudgetEstimation> estimations = await estimationsTask.ConfigureAwait(false);
        List<Goal> goals = await goalsTask.ConfigureAwait(false);
        List<Debt> debts = await debtsTask.ConfigureAwait(false);
        List<Expense> expenseList = await expensesTask.ConfigureAwait(false);
        List<Income> incomeList = await incomesTask.ConfigureAwait(false);
        List<Category> allCategories = await categoriesTask.ConfigureAwait(false);
        IEnumerable<Category> expenseCats = allCategories.Where(c => c.Type == CategoryType.Expenses);
        IEnumerable<Category> incomeCats = allCategories.Where(c => c.Type == CategoryType.Incomes);
        List<Transfer> transferList = await transfersTask.ConfigureAwait(false);

        List<ExchangeResponse>? cached = await exchangesCacheTask.ConfigureAwait(false);
        if (cached is null)
        {
            using var exchangesCtx = contextFactory.CreateReadOnlyContext();
            cached = (await exchangesCtx.QueryAsync(new ExchangesByQuerySpecification(), ct).ConfigureAwait(false))
                .ToResponses().ToList();
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
