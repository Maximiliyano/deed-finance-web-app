using Deed.Application.Auth;
using Deed.Application.UtilityBills.Commands.Delete;
using Deed.Application.UtilityBills.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.UtilityBills.Commands;

public sealed class DeleteUtilityBillCommandHandlerTests
{
    private readonly IUtilityBillRepository _repositoryMock = Substitute.For<IUtilityBillRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly DeleteUtilityBillCommandHandler _handler;

    public DeleteUtilityBillCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new DeleteUtilityBillCommandHandler(_repositoryMock, _unitOfWorkMock, _userMock);
    }

    [Fact]
    public async Task Handle_WhenBillExists_DeletesAndPersists()
    {
        // Arrange
        var bill = new UtilityBill(1)
        {
            Name = "Electric",
            EstimatedAmount = 500m,
            Currency = CurrencyType.UAH,
            DueDayOfMonth = 15,
            CapitalId = 1,
            CategoryId = 7
        };
        var command = new DeleteUtilityBillCommand(1);

        _repositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns(bill);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Received(1).Delete(bill);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBillNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new DeleteUtilityBillCommand(99);
        _repositoryMock.GetAsync(Arg.Any<UtilityBillByIdSpecification>()).Returns((UtilityBill?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("utility bill"));
        _repositoryMock.DidNotReceive().Delete(Arg.Any<UtilityBill>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
