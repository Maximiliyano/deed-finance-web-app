using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BudgetEstimations.Specifications;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Dashboard.Responses;
using Deed.Application.Exchanges;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Dashboard.Queries.GetSummary;

internal sealed class GetDashboardSummaryQueryHandler(
    IUser user,
    ICapitalRepository capitalRepository,
    IBudgetEstimationRepository budgetEstimationRepository,
    IExpenseRepository expenseRepository,
    IIncomeRepository incomeRepository,
    IUserSettingsRepository userSettingsRepository,
    IExchangeRateProvider exchangeRateProvider)
    : IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryResponse>
{
    private const CurrencyType DefaultCurrency = CurrencyType.UAH;

    public async Task<Result<DashboardSummaryResponse>> Handle(GetDashboardSummaryQuery query, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(user.Name);

        var currency = await ResolveCurrencyAsync(query.Currency, cancellationToken).ConfigureAwait(false);
        var converter = new CurrencyConverter(await exchangeRateProvider.GetLatestAsync(cancellationToken).ConfigureAwait(false));

        var capitals = await capitalRepository
            .GetAllAsync(new CapitalsByQueryParamsSpecification(user.Name), cancellationToken)
            .ConfigureAwait(false);

        var incomes = await incomeRepository
            .GetTotalsByCategoryAsync(user.Name, query.CapitalIds, cancellationToken)
            .ConfigureAwait(false);

        var expenses = await expenseRepository
            .GetTotalsByCategoryAsync(user.Name, query.CapitalIds, cancellationToken)
            .ConfigureAwait(false);

        var budgets = await GetBudgetsAsync(query, cancellationToken).ConfigureAwait(false);

        var includedCapitals = capitals.Where(c => c.IncludeInTotal).ToList();
        var incomesByCategory = Aggregate(incomes, converter, currency);
        var expensesByCategory = Aggregate(expenses, converter, currency);

        return new DashboardSummaryResponse(
            currency.ToString(),
            includedCapitals.Sum(c => converter.Convert(c.Balance, c.Currency, currency)),
            includedCapitals.Sum(c => converter.Convert(c.Balance, c.Currency, DefaultCurrency)),
            incomes.Sum(i => converter.Convert(i.Total, i.Currency, currency)),
            incomes.Sum(i => converter.Convert(i.Total, i.Currency, DefaultCurrency)),
            expenses.Sum(e => converter.Convert(e.Total, e.Currency, currency)),
            expenses.Sum(e => converter.Convert(e.Total, e.Currency, DefaultCurrency)),
            budgets.Sum(b => converter.Convert(b.BudgetAmount, b.BudgetCurrency, currency)),
            budgets.Sum(b => converter.Convert(b.BudgetAmount, b.BudgetCurrency, DefaultCurrency)),
            incomesByCategory,
            expensesByCategory);
    }

    private async Task<IReadOnlyList<BudgetEstimation>> GetBudgetsAsync(GetDashboardSummaryQuery query, CancellationToken cancellationToken)
    {
        if (query.PeriodStart is null || query.PeriodEnd is null)
        {
            return [];
        }

        var estimations = await budgetEstimationRepository
            .GetAllAsync(new BudgetEstimationsByUserSpecification(query.PeriodStart.Value, query.PeriodEnd.Value, user.Name!), cancellationToken)
            .ConfigureAwait(false);

        return [.. estimations.Where(e => query.CapitalIds is null || e.CapitalId.HasValue && query.CapitalIds.Contains(e.CapitalId.Value))];
    }

    private async Task<CurrencyType> ResolveCurrencyAsync(string? requested, CancellationToken cancellationToken)
    {
        if (requested is not null)
        {
            return Enum.Parse<CurrencyType>(requested);
        }

        var settings = await userSettingsRepository.GetAsync(user.Name!, cancellationToken).ConfigureAwait(false);

        return settings is null || settings.Currency == CurrencyType.None ? DefaultCurrency : settings.Currency;
    }

    private static List<CategoryAmountResponse> Aggregate(IEnumerable<CategoryAmount> rows, CurrencyConverter converter, CurrencyType currency)
    {
        var categories = rows
            .GroupBy(r => (r.CategoryId, r.CategoryName))
            .Select(g => (g.Key.CategoryId, g.Key.CategoryName, Amount: g.Sum(r => converter.Convert(r.Total, r.Currency, currency))))
            .Where(c => c.Amount > 0)
            .ToList();

        var total = categories.Sum(c => c.Amount);

        return [.. categories
            .OrderByDescending(c => c.Amount)
            .Select(c => new CategoryAmountResponse(c.CategoryId, c.CategoryName, c.Amount, total > 0 ? c.Amount / total * 100 : 0))];
    }
}
