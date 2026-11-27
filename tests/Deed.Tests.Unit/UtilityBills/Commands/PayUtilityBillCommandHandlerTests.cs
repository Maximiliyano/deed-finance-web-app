using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Exchanges;
using Deed.Application.Exchanges.Responses;
using Deed.Application.Expenses.Commands.Delete;
using Deed.Application.Expenses.Specifications;
using Deed.Application.UtilityBills.Commands.Pay;
using Deed.Application.UtilityBills.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.UtilityBills.Commands;

public sealed class PayUtilityBillCommandHandlerTests
{
    private readonly IUtilityBillRepository _billRepositoryMock = Substitute.For<IUtilityBillRepository>();
    private readonly ICapitalRepository _capitalRepositoryMock = Substitute.For<ICapitalRepository>();
    private readonly IExpenseRepository _expenseRepositoryMock = Substitute.For<IExpenseRepository>();
    private readonly IExchangeRateProvider _ratesMock = Substitute.For<IExchangeRateProvider>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly PayUtilityBillCommandHandler _handler;

    public PayUtilityBillCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new PayUtilityBillCommandHandler(
            _billRepositoryMock, _capitalRepositoryMock, _expenseRepositoryMock, _ratesMock, _unitOfWorkMock, _userMock);
    }

    [Fact]
    public async Task Handle_WhenValid_CreatesExpenseAndDeductsCapitalBalance()
    {
        // Arrange
        var bill = new UtilityBill(1)
        {
            Name = "Electric",
            EstimatedAmount = 500m,
            Currency = CurrencyType.UAH,
            DueDayOfMonth = 15,
            CapitalId = 42,
            CategoryId = 7
        };
        var capital = new Capital(42)
        {
            Name = "Checking",
            Balance = 1000m,
            Currency = CurrencyType.UAH,
            OnlyForSavings = false
        };
        var paymentDate = new DateTimeOffset(2026, 5, 14, 0, 0, 0, TimeSpan.Zero);
        var command = new PayUtilityBillCommand(1, 480m, paymentDate);

        _billRepositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(bill);
        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capital.Balance.Should().Be(520m);

        _expenseRepositoryMock.Received(1).Create(Arg.Is<Expense>(e =>
            e.Amount == 480m &&
            e.CategoryId == 7 &&
            e.CapitalId == 42 &&
            e.UtilityBillId == 1 &&
            e.Purpose == "Electric" &&
            e.PaymentDate == paymentDate
        ));
        _capitalRepositoryMock.Received(1).Update(capital);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCapitalCurrencyDiffers_ConvertsAmountIntoCapitalCurrency()
    {
        // Arrange
        var bill = CreateBill(CurrencyType.UAH);
        var capital = new Capital(42) { Name = "Euro card", Balance = 1000m, Currency = CurrencyType.EUR };
        var command = new PayUtilityBillCommand(1, 5000m, null);

        _billRepositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(bill);
        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);
        _ratesMock.GetLatestAsync(Arg.Any<CancellationToken>())
            .Returns([new ExchangeResponse("EUR", "UAH", 48m, 50m, null)]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capital.Balance.Should().Be(895.83m);

        _expenseRepositoryMock.Received(1).Create(Arg.Is<Expense>(e => e.Amount == 104.17m && e.CapitalId == 42));
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCurrenciesMatch_UsesAmountAsIsWithoutRateLookup()
    {
        // Arrange
        var bill = CreateBill(CurrencyType.EUR);
        var capital = new Capital(42) { Name = "Euro card", Balance = 1000m, Currency = CurrencyType.EUR };
        var command = new PayUtilityBillCommand(1, 120.55m, null);

        _billRepositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(bill);
        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capital.Balance.Should().Be(879.45m);

        _expenseRepositoryMock.Received(1).Create(Arg.Is<Expense>(e => e.Amount == 120.55m));
        await _ratesMock.DidNotReceive().GetLatestAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRateIsMissing_FailsWithoutChangingBalanceOrSaving()
    {
        // Arrange
        var bill = CreateBill(CurrencyType.UAH);
        var capital = new Capital(42) { Name = "Euro card", Balance = 1000m, Currency = CurrencyType.EUR };
        var command = new PayUtilityBillCommand(1, 5000m, null);

        _billRepositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(bill);
        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);
        _ratesMock.GetLatestAsync(Arg.Any<CancellationToken>()).Returns([]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.Exchange.RateNotFound);
        capital.Balance.Should().Be(1000m);

        _expenseRepositoryMock.DidNotReceive().Create(Arg.Any<Expense>());
        _capitalRepositoryMock.DidNotReceive().Update(Arg.Any<Capital>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPaymentIsDeleted_RestoresExactlyWhatWasDeducted()
    {
        // Arrange
        var bill = CreateBill(CurrencyType.UAH);
        var capital = new Capital(42) { Name = "Euro card", Balance = 1000m, Currency = CurrencyType.EUR };
        Expense? created = null;

        _billRepositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(bill);
        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);
        _ratesMock.GetLatestAsync(Arg.Any<CancellationToken>())
            .Returns([new ExchangeResponse("EUR", "UAH", 48m, 50m, null)]);
        _expenseRepositoryMock.When(r => r.Create(Arg.Any<Expense>())).Do(call => created = call.Arg<Expense>());

        await _handler.Handle(new PayUtilityBillCommand(1, 5000m, null), CancellationToken.None);

        var payment = new Expense(9)
        {
            Amount = created!.Amount,
            PaymentDate = DateTimeOffset.UtcNow,
            CategoryId = 7,
            CapitalId = 42,
            Capital = capital
        };
        var deleteRepositoryMock = Substitute.For<IExpenseRepository>();
        deleteRepositoryMock.GetAsync(Arg.Any<ExpenseByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(payment);
        var deleteHandler = new DeleteExpenseCommandHandler(
            _capitalRepositoryMock, deleteRepositoryMock, _unitOfWorkMock, _userMock);

        // Act
        var result = await deleteHandler.Handle(new DeleteExpenseCommand(9), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capital.Balance.Should().Be(1000m);
    }

    [Fact]
    public async Task Handle_WhenBillNotFound_ReturnsFailureAndCreatesNothing()
    {
        // Arrange
        var command = new PayUtilityBillCommand(99, 100m, null);
        _billRepositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns((UtilityBill?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("utility bill"));

        _expenseRepositoryMock.DidNotReceive().Create(Arg.Any<Expense>());
        _capitalRepositoryMock.DidNotReceive().Update(Arg.Any<Capital>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCapitalNotFound_ReturnsFailure()
    {
        // Arrange
        var bill = new UtilityBill(1)
        {
            Name = "Electric",
            EstimatedAmount = 100m,
            Currency = CurrencyType.UAH,
            DueDayOfMonth = 15,
            CapitalId = 42,
            CategoryId = 7
        };
        var command = new PayUtilityBillCommand(1, 100m, null);

        _billRepositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(bill);
        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns((Capital?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _expenseRepositoryMock.DidNotReceive().Create(Arg.Any<Expense>());
    }

    [Fact]
    public async Task Handle_WhenCapitalIsSavingsOnly_ReturnsFailure()
    {
        // Arrange
        var bill = new UtilityBill(1)
        {
            Name = "Electric",
            EstimatedAmount = 100m,
            Currency = CurrencyType.UAH,
            DueDayOfMonth = 15,
            CapitalId = 42,
            CategoryId = 7
        };
        var capital = new Capital(42)
        {
            Name = "Savings",
            Balance = 1000m,
            Currency = CurrencyType.UAH,
            OnlyForSavings = true
        };
        var command = new PayUtilityBillCommand(1, 100m, null);

        _billRepositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(bill);
        _capitalRepositoryMock.GetAsync(Arg.Any<CapitalByIdSpecification>()).Returns(capital);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.Capital.ForSavingsOnly);

        _expenseRepositoryMock.DidNotReceive().Create(Arg.Any<Expense>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static UtilityBill CreateBill(CurrencyType currency) => new(1)
    {
        Name = "Electric",
        EstimatedAmount = 500m,
        Currency = currency,
        DueDayOfMonth = 15,
        CapitalId = 42,
        CategoryId = 7
    };
}
