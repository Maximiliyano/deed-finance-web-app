using Deed.Application.Auth;
using Deed.Application.BudgetEstimations.Specifications;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Dashboard.Queries.GetSummary;
using Deed.Application.Exchanges;
using Deed.Application.Exchanges.Responses;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Dashboard;

public sealed class GetDashboardSummaryQueryHandlerTests
{
    private readonly IUser _user = Substitute.For<IUser>();
    private readonly ICapitalRepository _capitalRepository = Substitute.For<ICapitalRepository>();
    private readonly IBudgetEstimationRepository _budgetEstimationRepository = Substitute.For<IBudgetEstimationRepository>();
    private readonly IExpenseRepository _expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly IIncomeRepository _incomeRepository = Substitute.For<IIncomeRepository>();
    private readonly IUserSettingsRepository _userSettingsRepository = Substitute.For<IUserSettingsRepository>();
    private readonly IExchangeRateProvider _exchangeRateProvider = Substitute.For<IExchangeRateProvider>();

    private readonly GetDashboardSummaryQueryHandler _handler;

    public GetDashboardSummaryQueryHandlerTests()
    {
        _user.Name.Returns("testuser");
        _exchangeRateProvider.GetLatestAsync(Arg.Any<CancellationToken>())
            .Returns([new ExchangeResponse("USD", "UAH", 40m, 40m, null)]);
        _capitalRepository.GetAllAsync(Arg.Any<CapitalsByQueryParamsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                new Capital(1) { Name = "Wallet", Balance = 100m, Currency = CurrencyType.USD, IncludeInTotal = true },
                new Capital(2) { Name = "Cash", Balance = 400m, Currency = CurrencyType.UAH, IncludeInTotal = true },
                new Capital(3) { Name = "Savings", Balance = 999m, Currency = CurrencyType.UAH, IncludeInTotal = false }
            ]);
        _incomeRepository.GetTotalsByCategoryAsync(Arg.Any<string>(), Arg.Any<IReadOnlyCollection<int>?>(), Arg.Any<CancellationToken>())
            .Returns([new CategoryAmount(1, "Salary", CurrencyType.UAH, 800m), new CategoryAmount(1, "Salary", CurrencyType.USD, 10m)]);
        _expenseRepository.GetTotalsByCategoryAsync(Arg.Any<string>(), Arg.Any<IReadOnlyCollection<int>?>(), Arg.Any<CancellationToken>())
            .Returns([new CategoryAmount(2, "Food", CurrencyType.UAH, 300m), new CategoryAmount(3, "Rent", CurrencyType.UAH, 100m)]);

        _budgetEstimationRepository.GetAllAsync(Arg.Any<BudgetEstimationsByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                new BudgetEstimation { Description = "Rent", BudgetAmount = 10m, BudgetCurrency = CurrencyType.USD, CapitalId = 1 },
                new BudgetEstimation { Description = "Food", BudgetAmount = 200m, BudgetCurrency = CurrencyType.UAH, CapitalId = 2 }
            ]);

        _handler = new GetDashboardSummaryQueryHandler(
            _user, _capitalRepository, _budgetEstimationRepository, _expenseRepository, _incomeRepository, _userSettingsRepository, _exchangeRateProvider);
    }

    [Fact]
    public async Task Handle_ConvertsTotalsToRequestedCurrency()
    {
        // Arrange
        var query = new GetDashboardSummaryQuery("UAH");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Currency.Should().Be("UAH");
        result.Value.TotalCapital.Should().Be(4400m);
        result.Value.TotalIncome.Should().Be(1200m);
        result.Value.TotalExpense.Should().Be(400m);
        result.Value.IncomesByCategory.Should().ContainSingle().Which.Amount.Should().Be(1200m);
    }

    [Fact]
    public async Task Handle_SumsConvertedBudgets_WhenPeriodIsProvided()
    {
        // Arrange
        var query = new GetDashboardSummaryQuery("UAH", PeriodStart: DateTime.UtcNow.AddDays(-1), PeriodEnd: DateTime.UtcNow.AddDays(1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.TotalBudget.Should().Be(600m);
        result.Value.TotalBudgetInDefaultCurrency.Should().Be(600m);
    }

    [Fact]
    public async Task Handle_FiltersBudgetsByCapital_WhenCapitalIdsAreProvided()
    {
        // Arrange
        var query = new GetDashboardSummaryQuery("UAH", [2], DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.TotalBudget.Should().Be(200m);
    }

    [Fact]
    public async Task Handle_ReturnsZeroBudget_WhenPeriodIsMissing()
    {
        // Arrange
        // Act
        var result = await _handler.Handle(new GetDashboardSummaryQuery("UAH"), CancellationToken.None);

        // Assert
        result.Value.TotalBudget.Should().Be(0m);
        await _budgetEstimationRepository.DidNotReceive().GetAllAsync(Arg.Any<BudgetEstimationsByUserSpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsCategoriesSortedByAmountWithPercentages()
    {
        // Arrange
        var query = new GetDashboardSummaryQuery("UAH");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.ExpensesByCategory.Select(c => c.Name).Should().Equal("Food", "Rent");
        result.Value.ExpensesByCategory.Select(c => c.Percentage).Should().Equal(75m, 25m);
    }

    [Fact]
    public async Task Handle_UsesUserSettingsCurrency_WhenCurrencyIsNotRequested()
    {
        // Arrange
        _userSettingsRepository.GetAsync("testuser", Arg.Any<CancellationToken>())
            .Returns(new Deed.Domain.Entities.UserSettings { Currency = CurrencyType.USD });

        // Act
        var result = await _handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        // Assert
        result.Value.Currency.Should().Be("USD");
        result.Value.TotalCapital.Should().Be(110m);
        result.Value.TotalCapitalInDefaultCurrency.Should().Be(4400m);
    }

    [Fact]
    public async Task Handle_FallsBackToDefaultCurrency_WhenUserHasNoSettings()
    {
        // Arrange
        // Act
        var result = await _handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        // Assert
        result.Value.Currency.Should().Be("UAH");
        await _userSettingsRepository.Received(1).GetAsync("testuser", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PassesCapitalFilterToRepositories()
    {
        // Arrange
        int[] capitalIds = [1, 2];

        // Act
        await _handler.Handle(new GetDashboardSummaryQuery("UAH", capitalIds), CancellationToken.None);

        // Assert
        await _incomeRepository.Received(1).GetTotalsByCategoryAsync("testuser", capitalIds, Arg.Any<CancellationToken>());
        await _expenseRepository.Received(1).GetTotalsByCategoryAsync("testuser", capitalIds, Arg.Any<CancellationToken>());
        await _userSettingsRepository.DidNotReceive().GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
