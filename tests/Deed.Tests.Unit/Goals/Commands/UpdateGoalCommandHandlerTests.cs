using Deed.Application.Auth;
using Deed.Application.Goals.Commands.Update;
using Deed.Application.Goals.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Goals.Commands;

public sealed class UpdateGoalCommandHandlerTests
{
    private readonly IGoalRepository _repositoryMock = Substitute.For<IGoalRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly UpdateGoalCommandHandler _handler;

    public UpdateGoalCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new UpdateGoalCommandHandler(_repositoryMock, _unitOfWorkMock, _userMock);
    }

    [Fact]
    public async Task Handle_WhenGoalExists_ReturnsSuccess()
    {
        // Arrange
        var goal = new Goal(1)
        {
            Title = "Old",
            TargetAmount = 1000m,
            Currency = CurrencyType.USD
        };
        var command = new UpdateGoalCommand(
            1, "New Title", 5000m, CurrencyType.EUR,
            1000m, null, "note", true);

        _repositoryMock.GetAsync(Arg.Any<GoalByIdSpecification>()).Returns(goal);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        goal.Title.Should().Be("New Title");
        goal.TargetAmount.Should().Be(5000m);
        goal.IsCompleted.Should().BeTrue();

        _repositoryMock.Received(1).Update(goal);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGoalNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new UpdateGoalCommand(
            99, "Title", 100m, CurrencyType.UAH,
            0m, null, null, false);

        _repositoryMock.GetAsync(Arg.Any<GoalByIdSpecification>()).Returns((Goal?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e == DomainErrors.General.NotFound("goal"));

        _repositoryMock.DidNotReceive().Update(Arg.Any<Goal>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
