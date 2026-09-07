using Deed.Application.Auth;
using Deed.Application.UtilityBills.Commands.Create;
using Deed.Application.UtilityBills.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.UtilityBills.Commands;

public sealed class CreateUtilityBillCommandHandlerTests
{
    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IUtilityBillRepository _repositoryMock = Substitute.For<IUtilityBillRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly CreateUtilityBillCommandHandler _handler;

    public CreateUtilityBillCommandHandlerTests()
    {
        _userMock.IsAuthenticated.Returns(true);
        _handler = new CreateUtilityBillCommandHandler(_userMock, _repositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_CreateValidBill_ReturnsSuccessAndPersists()
    {
        // Arrange
        var command = new CreateUtilityBillCommand(
            "  Electric  ", 500m, CurrencyType.UAH, 15, 1, 7, true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Received(1).Create(Arg.Is<UtilityBill>(b =>
            b.Name == "Electric" &&
            b.EstimatedAmount == 500m &&
            b.Currency == CurrencyType.UAH &&
            b.DueDayOfMonth == 15 &&
            b.CapitalId == 1 &&
            b.CategoryId == 7 &&
            b.IsActive
        ));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AnonymousUserOverLimit_ReturnsFailureAndDoesNotPersist()
    {
        // Arrange
        _userMock.IsAuthenticated.Returns(false);
        _userMock.Name.Returns("anon");
        _repositoryMock
            .CountAsync(Arg.Any<UtilityBillsByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(AuthConstants.EntityLimit);

        var command = new CreateUtilityBillCommand(
            "Water", 100m, CurrencyType.UAH, 5, 1, 7, true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.Anonymous.LimitReached);

        _repositoryMock.DidNotReceive().Create(Arg.Any<UtilityBill>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
