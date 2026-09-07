using Deed.Application.Auth;
using Deed.Application.UtilityBills.Commands.Update;
using Deed.Application.UtilityBills.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.UtilityBills.Commands;

public sealed class UpdateUtilityBillCommandHandlerTests
{
    private readonly IUtilityBillRepository _repositoryMock = Substitute.For<IUtilityBillRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly UpdateUtilityBillCommandHandler _handler;

    public UpdateUtilityBillCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new UpdateUtilityBillCommandHandler(_repositoryMock, _unitOfWorkMock, _userMock);
    }

    [Fact]
    public async Task Handle_WhenBillExists_AppliesUpdateAndPersists()
    {
        // Arrange
        var bill = new UtilityBill(1)
        {
            Name = "Old",
            EstimatedAmount = 100m,
            Currency = CurrencyType.UAH,
            DueDayOfMonth = 10,
            CapitalId = 1,
            CategoryId = 5,
            IsActive = true
        };
        var command = new UpdateUtilityBillCommand(
            1, "  New Name  ", 250m, CurrencyType.USD, 20, 2, 6, false);

        _repositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(bill);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        bill.Name.Should().Be("New Name");
        bill.EstimatedAmount.Should().Be(250m);
        bill.Currency.Should().Be(CurrencyType.USD);
        bill.DueDayOfMonth.Should().Be(20);
        bill.CapitalId.Should().Be(2);
        bill.CategoryId.Should().Be(6);
        bill.IsActive.Should().BeFalse();

        _repositoryMock.Received(1).Update(bill);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBillNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new UpdateUtilityBillCommand(
            99, "X", 1m, CurrencyType.UAH, 1, 1, 1, true);
        _repositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns((UtilityBill?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("utility bill"));

        _repositoryMock.DidNotReceive().Update(Arg.Any<UtilityBill>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoFieldsChanged_SkipsUpdateAndPersist()
    {
        // Arrange
        var bill = new UtilityBill(1)
        {
            Name = "Same",
            EstimatedAmount = 100m,
            Currency = CurrencyType.UAH,
            DueDayOfMonth = 10,
            CapitalId = 1,
            CategoryId = 5,
            IsActive = true
        };
        var command = new UpdateUtilityBillCommand(
            1, "Same", 100m, CurrencyType.UAH, 10, 1, 5, true);

        _repositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(bill);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.DidNotReceive().Update(Arg.Any<UtilityBill>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
