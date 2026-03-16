using Deed.Application.Auth;
using Deed.Application.Goals.Commands.Delete;
using Deed.Application.Goals.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Goals.Commands;

public sealed class DeleteGoalCommandHandlerTests
{
    private readonly IGoalRepository _repositoryMock = Substitute.For<IGoalRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly DeleteGoalCommandHandler _handler;

    public DeleteGoalCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new DeleteGoalCommandHandler(_repositoryMock, _unitOfWorkMock, _userMock);
    }

    [Fact]
    public async Task Handle_WhenGoalExists_ReturnsSuccess()
    {
        // Arrange
        var goal = new Goal(1) { Title = "Test", TargetAmount = 100m, Currency = CurrencyType.UAH };
        var command = new DeleteGoalCommand(1);

        _repositoryMock.GetAsync(Arg.Any<GoalByIdSpecification>()).Returns(goal);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Received(1).Delete(goal);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGoalNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new DeleteGoalCommand(99);
        _repositoryMock.GetAsync(Arg.Any<GoalByIdSpecification>()).Returns((Goal?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("goal"));
        _repositoryMock.Received(0).Delete(Arg.Any<Goal>());
        await _unitOfWorkMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
