using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Exchanges;
using Deed.Application.UtilityBills.Commands.Pay;
using Deed.Application.UtilityBills.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.UtilityBills.Commands;

public sealed class PayMeteredUtilityBillCommandHandlerTests
{
    private static readonly DateTimeOffset PaymentDate = new(2026, 6, 14, 0, 0, 0, TimeSpan.Zero);

    private readonly IUtilityBillRepository _billRepositoryMock = Substitute.For<IUtilityBillRepository>();
    private readonly ICapitalRepository _capitalRepositoryMock = Substitute.For<ICapitalRepository>();
    private readonly IExpenseRepository _expenseRepositoryMock = Substitute.For<IExpenseRepository>();
    private readonly IExchangeRateProvider _exchangeRateProviderMock = Substitute.For<IExchangeRateProvider>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly PayUtilityBillCommandHandler _handler;

    public PayMeteredUtilityBillCommandHandlerTests()
    {
        _userMock.Id.Returns("testuser");
        _handler = new PayUtilityBillCommandHandler(
            _billRepositoryMock,
            _capitalRepositoryMock,
            _expenseRepositoryMock,
            _exchangeRateProviderMock,
            _unitOfWorkMock,
            _userMock);
    }

    private static UtilityBill MeteredBill() => new(1)
    {
        Name = "Electric",
        EstimatedAmount = 0m,
        Currency = CurrencyType.UAH,
        DueDayOfMonth = 15,
        CapitalId = 42,
        CategoryId = 7,
        UnitPrice = 4.32m,
        Unit = "kWh"
    };

    private static Capital UahCapital() => new(42)
    {
        Name = "Checking",
        Balance = 5000m,
        Currency = CurrencyType.UAH
    };

    private void SetupPreviousReading(decimal? reading)
    {
        var readings = reading is null
            ? new Dictionary<int, decimal>()
            : new Dictionary<int, decimal> { [1] = reading.Value };

        _expenseRepositoryMock
            .GetLatestMeterReadingsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(readings);
    }

    [Fact]
    public async Task Handle_WhenMeteredWithPreviousReading_ChargesConsumptionOnly()
    {
        // Arrange
        var bill = MeteredBill();
        var capital = UahCapital();
        var command = new PayUtilityBillCommand(1, 0m, PaymentDate, 1350m);

        _billRepositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(bill);
        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);
        SetupPreviousReading(1200m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capital.Balance.Should().Be(5000m - 648m);

        _expenseRepositoryMock.Received(1).Create(Arg.Is<Expense>(e =>
            e.Amount == 648m &&
            e.MeterReading == 1350m &&
            e.UtilityBillId == 1 &&
            e.CapitalId == 42));
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMeteredWithoutPreviousMonthPayment_UsesZeroAsPreviousReading()
    {
        // Arrange
        var bill = MeteredBill();
        var capital = UahCapital();
        var command = new PayUtilityBillCommand(1, 0m, PaymentDate, 300m);

        _billRepositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(bill);
        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);
        SetupPreviousReading(null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _expenseRepositoryMock.Received(1).Create(Arg.Is<Expense>(e =>
            e.Amount == 1296m &&
            e.MeterReading == 300m));
    }

    [Fact]
    public async Task Handle_WhenMetered_LooksUpReadingsInThePreviousCalendarMonth()
    {
        // Arrange
        var bill = MeteredBill();
        var command = new PayUtilityBillCommand(1, 0m, PaymentDate, 1350m);

        _billRepositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(bill);
        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(UahCapital());
        SetupPreviousReading(1200m);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _expenseRepositoryMock.Received(1).GetLatestMeterReadingsAsync(
            Arg.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(1)),
            new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(1200)]
    [InlineData(1100)]
    public async Task Handle_WhenReadingIsNotAbovePrevious_ReturnsFailureAndChangesNothing(int reading)
    {
        // Arrange
        var capital = UahCapital();
        var command = new PayUtilityBillCommand(1, 0m, PaymentDate, reading);

        _billRepositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(MeteredBill());
        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);
        SetupPreviousReading(1200m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.UtilityBill.MeterReadingNotIncreasing);
        capital.Balance.Should().Be(5000m);

        _expenseRepositoryMock.DidNotReceive().Create(Arg.Any<Expense>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMeteredAndReadingMissing_ReturnsFailure()
    {
        // Arrange
        var command = new PayUtilityBillCommand(1, 500m, PaymentDate);

        _billRepositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(MeteredBill());
        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(UahCapital());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.UtilityBill.MeterReadingRequired);

        _expenseRepositoryMock.DidNotReceive().Create(Arg.Any<Expense>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotMetered_IgnoresReadingAndUsesSubmittedAmount()
    {
        // Arrange
        var bill = MeteredBill();
        bill.UnitPrice = null;
        bill.Unit = null;
        var capital = UahCapital();
        var command = new PayUtilityBillCommand(1, 480m, PaymentDate, 999m);

        _billRepositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(bill);
        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capital.Balance.Should().Be(4520m);

        _expenseRepositoryMock.Received(1).Create(Arg.Is<Expense>(e =>
            e.Amount == 480m &&
            e.MeterReading == null));
        await _expenseRepositoryMock.DidNotReceive().GetLatestMeterReadingsAsync(
            Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }
}
