using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
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
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly PayUtilityBillCommandHandler _handler;

    public PayUtilityBillCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new PayUtilityBillCommandHandler(
            _billRepositoryMock, _capitalRepositoryMock, _expenseRepositoryMock, _unitOfWorkMock, _userMock);
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
}
