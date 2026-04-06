using Deed.Application.Abstractions.Caching;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Auth;
using Deed.Application.Dashboard;
using Deed.Application.Exchanges.Specifications;
using Deed.Application.Expenses.Responses;
using Deed.Application.Transfers.Responses;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using DomainUserSettings = Deed.Domain.Entities.UserSettings;

namespace Deed.Tests.Unit.Dashboard.Queries;

public sealed class GetDashboardQueryHandlerTests
{
    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly ICapitalRepository _capitalRepoMock = Substitute.For<ICapitalRepository>();
    private readonly IExchangeRepository _exchangeRepoMock = Substitute.For<IExchangeRepository>();
    private readonly IBudgetEstimationRepository _estimationRepoMock = Substitute.For<IBudgetEstimationRepository>();
    private readonly IGoalRepository _goalRepoMock = Substitute.For<IGoalRepository>();
    private readonly IDebtRepository _debtRepoMock = Substitute.For<IDebtRepository>();
    private readonly IExpenseRepository _expenseRepoMock = Substitute.For<IExpenseRepository>();
    private readonly IIncomeRepository _incomeRepoMock = Substitute.For<IIncomeRepository>();
    private readonly ICategoryRepository _categoryRepoMock = Substitute.For<ICategoryRepository>();
    private readonly IUserSettingsRepository _settingsRepoMock = Substitute.For<IUserSettingsRepository>();
    private readonly ITransferRepository _transferRepoMock = Substitute.For<ITransferRepository>();
    private readonly ICacheService _cacheServiceMock = Substitute.For<ICacheService>();

    private readonly GetDashboardQueryHandler _handler;

    public GetDashboardQueryHandlerTests()
    {
        _userMock.Name.Returns("testuser");

        IOptions<MemoryCacheSettings> cacheSettings = Options.Create(new MemoryCacheSettings
        {
            ExchangesTimespanInHours = 1,
            CategoriesTimespanInHours = 1
        });

        _handler = new GetDashboardQueryHandler(
            _userMock,
            _capitalRepoMock,
            _exchangeRepoMock,
            _estimationRepoMock,
            _goalRepoMock,
            _debtRepoMock,
            _expenseRepoMock,
            _incomeRepoMock,
            _categoryRepoMock,
            _settingsRepoMock,
            _transferRepoMock,
            _cacheServiceMock,
            cacheSettings);

        SetupEmptyDefaults();
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WithAllData()
    {
        // Arrange
        DomainUserSettings settings = new() { Salary = 5000m, Currency = CurrencyType.USD };
        Capital capital = new(1) { Name = "Cash", Balance = 1000m, Currency = CurrencyType.UAH };
        BudgetEstimation estimation = new(1)
            { Description = "Rent", BudgetAmount = 500m, BudgetCurrency = CurrencyType.UAH };
        Goal goal = new(1) { Title = "Trip", TargetAmount = 2000m, Currency = CurrencyType.USD };
        Debt debt = new(1)
        {
            Item = "Loan", Amount = 100m, Currency = CurrencyType.UAH, Source = "Bank", Recipient = "Me",
            BorrowedAt = DateTimeOffset.UtcNow
        };

        _settingsRepoMock.GetAsync("testuser", Arg.Any<CancellationToken>()).Returns(settings);
        _capitalRepoMock.GetAllAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { capital });
        _estimationRepoMock.GetAllAsync(Arg.Any<ISpecification<BudgetEstimation>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { estimation });
        _goalRepoMock.GetAllAsync(Arg.Any<ISpecification<Goal>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { goal });
        _debtRepoMock.GetAllAsync(Arg.Any<ISpecification<Debt>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { debt });

        // Act
        Result<DashboardResponse> result = await _handler.Handle(new GetDashboardQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Settings.Should().NotBeNull();
        result.Value.Settings!.Salary.Should().Be(5000m);
        result.Value.Capitals.Should().HaveCount(1);
        result.Value.Estimations.Should().HaveCount(1);
        result.Value.Goals.Should().HaveCount(1);
        result.Value.Debts.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenAllEmpty()
    {
        // Act
        Result<DashboardResponse> result = await _handler.Handle(new GetDashboardQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Settings.Should().BeNull();
        result.Value.Capitals.Should().BeEmpty();
        result.Value.Estimations.Should().BeEmpty();
        result.Value.Goals.Should().BeEmpty();
        result.Value.Debts.Should().BeEmpty();
        result.Value.ExpenseCategories.Should().BeEmpty();
        result.Value.Incomes.Should().BeEmpty();
        result.Value.Transfers.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ThrowsArgumentNullException_WhenUserNameIsNull()
    {
        // Arrange
        _userMock.Name.Returns((string?)null);

        // Act
        Func<Task<Result<DashboardResponse>>> act = () =>
            _handler.Handle(new GetDashboardQuery(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_GroupsExpensesByCategory_WithCorrectPercentages()
    {
        // Arrange
        Category category1 = new(1) { Name = "Food", Type = CategoryType.Expenses, Period = PerPeriodType.None };
        Category category2 = new(2) { Name = "Rent", Type = CategoryType.Expenses, Period = PerPeriodType.None };

        List<Expense> expenses = new()
        {
            new Expense
            {
                Amount = 300m, PaymentDate = DateTimeOffset.UtcNow, CategoryId = 1, CapitalId = 1, Category = category1
            },
            new Expense
            {
                Amount = 200m, PaymentDate = DateTimeOffset.UtcNow, CategoryId = 1, CapitalId = 1, Category = category1
            },
            new Expense
            {
                Amount = 500m, PaymentDate = DateTimeOffset.UtcNow, CategoryId = 2, CapitalId = 1, Category = category2
            }
        };

        _expenseRepoMock.GetAllAsync(Arg.Any<ISpecification<Expense>>(), Arg.Any<CancellationToken>())
            .Returns(expenses);

        // Act
        Result<DashboardResponse> result = await _handler.Handle(new GetDashboardQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        List<CategoryExpenseResponse> categories = result.Value.ExpenseCategories.ToList();
        categories.Should().HaveCount(2);

        CategoryExpenseResponse food = categories.First(c => c.Name == "Food");
        food.CategorySum.Should().Be(500m);
        food.Percentage.Should().Be(50m);

        CategoryExpenseResponse rent = categories.First(c => c.Name == "Rent");
        rent.CategorySum.Should().Be(500m);
        rent.Percentage.Should().Be(50m);
    }

    [Fact]
    public async Task Handle_MapsTransfers_WithCapitalNames()
    {
        // Arrange
        Capital source = new(1) { Name = "Cash", Balance = 1000m, Currency = CurrencyType.UAH };
        Capital dest = new(2) { Name = "Bank", Balance = 2000m, Currency = CurrencyType.USD };
        Transfer transfer = new()
        {
            Amount = 100m,
            DestinationAmount = 2.5m,
            SourceCapitalId = 1,
            SourceCapital = source,
            DestinationCapitalId = 2,
            DestinationCapital = dest,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _transferRepoMock.GetAllAsync(Arg.Any<ISpecification<Transfer>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { transfer });

        // Act
        Result<DashboardResponse> result = await _handler.Handle(new GetDashboardQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        TransferResponse t = result.Value.Transfers.First();
        t.SourceCapitalName.Should().Be("Cash");
        t.DestinationCapitalName.Should().Be("Bank");
        t.Amount.Should().Be(100m);
        t.DestinationAmount.Should().Be(2.5m);
    }

    [Fact]
    public async Task Handle_FetchesExchangesFromRepository_WhenCacheMiss()
    {
        // Arrange
        List<Exchange> exchanges = new()
        {
            new Exchange
            {
                NationalCurrencyCode = "UAH", TargetCurrencyCode = "USD", Buy = 41m, Sale = 41.5m,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        _exchangeRepoMock.GetAllAsync(Arg.Any<ExchangesByQuerySpecification>(), Arg.Any<CancellationToken>())
            .Returns(exchanges);

        // Act
        Result<DashboardResponse> result = await _handler.Handle(new GetDashboardQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Exchanges.Should().HaveCount(1);
        result.Value.Exchanges.First().TargetCurrency.Should().Be("USD");
    }

    private void SetupEmptyDefaults()
    {
        _settingsRepoMock.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((DomainUserSettings?)null);
        _capitalRepoMock.GetAllAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Capital>());
        _exchangeRepoMock.GetAllAsync(Arg.Any<ExchangesByQuerySpecification>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Exchange>());
        _estimationRepoMock.GetAllAsync(Arg.Any<ISpecification<BudgetEstimation>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<BudgetEstimation>());
        _goalRepoMock.GetAllAsync(Arg.Any<ISpecification<Goal>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Goal>());
        _debtRepoMock.GetAllAsync(Arg.Any<ISpecification<Debt>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Debt>());
        _expenseRepoMock.GetAllAsync(Arg.Any<ISpecification<Expense>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Expense>());
        _incomeRepoMock.GetAllAsync(Arg.Any<ISpecification<Income>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Income>());
        _categoryRepoMock.GetAllAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Category>());
        _transferRepoMock.GetAllAsync(Arg.Any<ISpecification<Transfer>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Transfer>());
    }
}
