using Deed.Application.Auth;
using Deed.Application.Goals.Commands.Create;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Goals.Commands;

public sealed class CreateGoalCommandHandlerTests
{
    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IGoalRepository _repositoryMock = Substitute.For<IGoalRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly CreateGoalCommandHandler _handler;

    public CreateGoalCommandHandlerTests()
    {
        _userMock.IsAuthenticated.Returns(true);
        _handler = new CreateGoalCommandHandler(_userMock, _repositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_CreateValidGoal_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateGoalCommand(
            "Emergency Fund  ", 10000m, CurrencyType.USD,
            500m, null, "  Save for rainy days  ");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().OnlyContain(c => c.Equals(Error.None));

        _repositoryMock.Received(1).Create(Arg.Is<Goal>(g =>
            g.Title == "Emergency Fund" &&
            g.TargetAmount == 10000m &&
            g.Currency == CurrencyType.USD &&
            g.CurrentAmount == 500m &&
            g.Note == "Save for rainy days"
        ));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
