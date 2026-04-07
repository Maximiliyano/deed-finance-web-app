using Deed.Application.Abstractions.Caching;
using Deed.Application.Abstractions.Data;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Auth;
using Deed.Application.Dashboard;
using Deed.Application.Exchanges.Responses;
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
    private readonly IUserSettingsRepository _settingsRepoMock = Substitute.For<IUserSettingsRepository>();
    private readonly IDeedDbContextFactory _contextFactoryMock = Substitute.For<IDeedDbContextFactory>();
    private readonly IDeedDbContext _dbContextMock = Substitute.For<IDeedDbContext>();
    private readonly ICacheService _cacheServiceMock = Substitute.For<ICacheService>();

    private readonly GetDashboardQueryHandler _handler;

    public GetDashboardQueryHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _contextFactoryMock.CreateReadOnlyContext().Returns(_dbContextMock);

        IOptions<MemoryCacheSettings> cacheSettings = Options.Create(new MemoryCacheSettings
        {
            ExchangesTimespanInHours = 1,
            CategoriesTimespanInHours = 1
        });

        _handler = new GetDashboardQueryHandler(
            _userMock,
            _contextFactoryMock,
            _settingsRepoMock,
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
        _dbContextMock.QueryAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Capital> { capital });
        _dbContextMock.QueryAsync(Arg.Any<ISpecification<BudgetEstimation>>(), Arg.Any<CancellationToken>())
            .Returns(new List<BudgetEstimation> { estimation });
        _dbContextMock.QueryAsync(Arg.Any<ISpecification<Goal>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Goal> { goal });
        _dbContextMock.QueryAsync(Arg.Any<ISpecification<Debt>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Debt> { debt });

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

        List<Expense> expenses =
        [
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
        ];

        _dbContextMock.QueryAsync(Arg.Any<ISpecification<Expense>>(), Arg.Any<CancellationToken>())
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

        _dbContextMock.QueryAsync(Arg.Any<ISpecification<Transfer>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Transfer> { transfer });

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
    public async Task Handle_FetchesExchangesFromDb_WhenCacheMiss()
    {
        // Arrange
        List<Exchange> exchanges =
        [
            new Exchange
            {
                NationalCurrencyCode = "UAH", TargetCurrencyCode = "USD", Buy = 41m, Sale = 41.5m,
                CreatedAt = DateTimeOffset.UtcNow
            }
        ];

        _dbContextMock.QueryAsync(Arg.Any<ISpecification<Exchange>>(), Arg.Any<CancellationToken>())
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

        _dbContextMock.QueryAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Capital>());
        _dbContextMock.QueryAsync(Arg.Any<ISpecification<BudgetEstimation>>(), Arg.Any<CancellationToken>())
            .Returns(new List<BudgetEstimation>());
        _dbContextMock.QueryAsync(Arg.Any<ISpecification<Goal>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Goal>());
        _dbContextMock.QueryAsync(Arg.Any<ISpecification<Debt>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Debt>());
        _dbContextMock.QueryAsync(Arg.Any<ISpecification<Expense>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Expense>());
        _dbContextMock.QueryAsync(Arg.Any<ISpecification<Income>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Income>());
        _dbContextMock.QueryAsync(Arg.Any<ISpecification<Category>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Category>());
        _dbContextMock.QueryAsync(Arg.Any<ISpecification<Transfer>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Transfer>());
        _dbContextMock.QueryAsync(Arg.Any<ISpecification<Exchange>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Exchange>());
    }
}
