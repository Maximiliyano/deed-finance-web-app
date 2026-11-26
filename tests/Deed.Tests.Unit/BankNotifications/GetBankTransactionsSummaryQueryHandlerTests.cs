using Deed.Application.Auth;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.BankNotifications.Queries.GetSummary;
using Deed.Application.BankNotifications.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankNotifications;

public sealed class GetBankTransactionsSummaryQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IBankNotificationRepository _repositoryMock = Substitute.For<IBankNotificationRepository>();
    private readonly IBankConnectionRepository _connectionsMock = Substitute.For<IBankConnectionRepository>();
    private readonly IDateTimeProvider _clockMock = Substitute.For<IDateTimeProvider>();

    private readonly GetBankTransactionsSummaryQueryHandler _handler;

    public GetBankTransactionsSummaryQueryHandlerTests()
    {
        _userMock.Name.Returns("owner");
        _clockMock.UtcNow.Returns(Now);
        _connectionsMock
            .GetAllAsync(Arg.Any<BankConnectionByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns([new BankConnection(1) { EncryptedToken = "x", AccountIban = "UA1", Label = "Main" }]);
        _repositoryMock
            .GetCounterpartyTotalsAsync(Arg.Any<BankTransactionsSummarySpecification>(), Arg.Any<CancellationToken>())
            .Returns([]);

        _handler = new GetBankTransactionsSummaryQueryHandler(_userMock, _repositoryMock, _connectionsMock, _clockMock);
    }

    [Fact]
    public async Task Handle_MonthPeriod_GroupsPerCurrencyAndMonth()
    {
        // Arrange
        ReturnTotals(
            Total(1, "UAH", BankTransactionDirection.Incoming, BankTransactionType.TopUp, 2026, 11, 3, 1000m, 1),
            Total(1, "UAH", BankTransactionDirection.Outgoing, BankTransactionType.Payment, 2026, 11, 20, 250m, 4),
            Total(1, "UAH", BankTransactionDirection.Outgoing, BankTransactionType.Exchange, 2026, 10, 5, 100m, 1),
            Total(1, "USD", BankTransactionDirection.Incoming, BankTransactionType.Exchange, 2026, 10, 5, 2m, 1));

        // Act
        var result = await _handler.Handle(new GetBankTransactionsSummaryQuery("month"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Period.Should().Be("Month");
        result.Value.From.Should().Be(new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.Zero));
        result.Value.Currencies.Select(c => c.Currency).Should().Equal("UAH", "USD");

        var uah = result.Value.Currencies[0];
        uah.Income.Should().Be(1000m);
        uah.Expense.Should().Be(350m);
        uah.Count.Should().Be(6);
        uah.Periods.Select(p => p.Start).Should().Equal(new DateOnly(2026, 11, 1), new DateOnly(2026, 10, 1));
        uah.Periods[0].Income.Should().Be(1000m);
        uah.Periods[0].Expense.Should().Be(250m);
        uah.Types.Select(t => t.Type).Should().Equal("TopUp", "Payment", "Exchange");
    }

    [Theory]
    [InlineData("day", 2026, 11, 20)]
    [InlineData("year", 2026, 1, 1)]
    public async Task Handle_Period_BucketsByDayOrYear(string period, int year, int month, int day)
    {
        // Arrange
        ReturnTotals(Total(1, "UAH", BankTransactionDirection.Outgoing, BankTransactionType.Payment, 2026, 11, 20, 50m, 1));

        // Act
        var result = await _handler.Handle(new GetBankTransactionsSummaryQuery(period), CancellationToken.None);

        // Assert
        result.Value.Currencies.Single().Periods.Single().Start.Should().Be(new DateOnly(year, month, day));
    }

    [Fact]
    public async Task Handle_TransactionsOfSeveralAccounts_SplitsPerAccount()
    {
        // Arrange
        ReturnTotals(
            Total(1, "UAH", BankTransactionDirection.Incoming, BankTransactionType.TopUp, 2026, 11, 3, 100m, 1),
            Total(null, "UAH", BankTransactionDirection.Outgoing, BankTransactionType.Payment, 2026, 11, 4, 30m, 2));

        // Act
        var result = await _handler.Handle(new GetBankTransactionsSummaryQuery(), CancellationToken.None);

        // Assert
        var accounts = result.Value.Currencies.Single().Accounts;
        accounts.Select(a => a.Label).Should().Equal("Main", "Other");
        accounts[0].ConnectionId.Should().Be(1);
        accounts[0].Income.Should().Be(100m);
        accounts[1].ConnectionId.Should().BeNull();
        accounts[1].Expense.Should().Be(30m);
    }

    [Fact]
    public async Task Handle_Counterparties_ReturnsTopFiveMergedByNameAndDirection()
    {
        // Arrange
        ReturnTotals(Total(1, "UAH", BankTransactionDirection.Outgoing, BankTransactionType.Payment, 2026, 11, 4, 600m, 6));
        var rows = Enumerable.Range(1, 6)
            .Select(i => new BankCounterpartyTotal("UAH", BankTransactionDirection.Outgoing, $"Shop {i}", i * 10m, 1))
            .Append(new BankCounterpartyTotal("UAH", BankTransactionDirection.Outgoing, "shop 6", 5m, 1))
            .Append(new BankCounterpartyTotal("UAH", BankTransactionDirection.Outgoing, null, 1m, 1))
            .Append(new BankCounterpartyTotal("UAH", BankTransactionDirection.Incoming, "Employer", 900m, 1))
            .Append(new BankCounterpartyTotal("USD", BankTransactionDirection.Outgoing, "Other currency", 999m, 1))
            .ToList();
        _repositoryMock
            .GetCounterpartyTotalsAsync(Arg.Any<BankTransactionsSummarySpecification>(), Arg.Any<CancellationToken>())
            .Returns(rows);

        // Act
        var result = await _handler.Handle(new GetBankTransactionsSummaryQuery(), CancellationToken.None);

        // Assert
        var uah = result.Value.Currencies.Single();
        uah.TopExpense.Should().HaveCount(5);
        uah.TopExpense[0].Name.Should().Be("Shop 6");
        uah.TopExpense[0].Amount.Should().Be(65m);
        uah.TopExpense.Select(c => c.Name).Should().NotContain("Other currency");
        uah.TopIncome.Single().Name.Should().Be("Employer");
    }

    [Fact]
    public async Task Handle_ConnectionFilter_PassesItToRepository()
    {
        // Arrange
        ReturnTotals();

        // Act
        await _handler.Handle(new GetBankTransactionsSummaryQuery(ConnectionId: 1), CancellationToken.None);

        // Assert
        await _repositoryMock.Received(1).GetTotalsAsync(Arg.Any<BankTransactionsSummarySpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoTransactions_ReturnsEmptyCurrencies()
    {
        // Arrange
        ReturnTotals();

        // Act
        var result = await _handler.Handle(new GetBankTransactionsSummaryQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Currencies.Should().BeEmpty();
    }

    private void ReturnTotals(params BankTransactionTotal[] totals)
        => _repositoryMock
            .GetTotalsAsync(Arg.Any<BankTransactionsSummarySpecification>(), Arg.Any<CancellationToken>())
            .Returns(totals);

    private static BankTransactionTotal Total(
        int? connectionId,
        string currency,
        BankTransactionDirection direction,
        BankTransactionType type,
        int year,
        int month,
        int day,
        decimal amount,
        int count)
        => new(connectionId, currency, direction, type, year, month, day, amount, count);
}
