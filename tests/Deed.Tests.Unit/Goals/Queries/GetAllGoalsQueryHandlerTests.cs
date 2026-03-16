using Deed.Application.Auth;
using Deed.Application.Goals;
using Deed.Application.Goals.Queries.GetAll;
using Deed.Application.Goals.Responses;
using Deed.Application.Goals.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Goals.Queries;

public sealed class GetAllGoalsQueryHandlerTests
{
    private readonly IGoalRepository _repositoryMock = Substitute.For<IGoalRepository>();
    private readonly IUser _userMock = Substitute.For<IUser>();

    private readonly GetAllGoalsQueryHandler _handler;

    public GetAllGoalsQueryHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new GetAllGoalsQueryHandler(_repositoryMock, _userMock);
    }

    [Fact]
    public async Task Handle_ReturnsGoals_WhenGoalsExist()
    {
        // Arrange
        var query = new GetAllGoalsQuery();
        var goals = new List<Goal>
        {
            new(1) { Title = "Emergency Fund", TargetAmount = 10000m, Currency = CurrencyType.USD }
        };
        var expected = goals.ToResponses();

        _repositoryMock.GetAllAsync(Arg.Any<GoalsByUserSpecification>()).Returns(goals);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expected);
        await _repositoryMock.Received(1).GetAllAsync(Arg.Any<GoalsByUserSpecification>());
    }

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenNoGoals()
    {
        // Arrange
        var query = new GetAllGoalsQuery();
        _repositoryMock.GetAllAsync(Arg.Any<GoalsByUserSpecification>()).Returns(new List<Goal>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(Enumerable.Empty<GoalResponse>());
        await _repositoryMock.Received(1).GetAllAsync(Arg.Any<GoalsByUserSpecification>());
    }
}
