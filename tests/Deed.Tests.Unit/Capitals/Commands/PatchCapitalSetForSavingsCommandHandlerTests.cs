using Deed.Application.Auth;
using Deed.Application.Capitals.Commands.PatchOnlyForSavings;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Capitals.Commands;

public sealed class PatchCapitalSetForSavingsCommandHandlerTests
{
    private readonly ICapitalRepository _repository = Substitute.For<ICapitalRepository>();
    private readonly IUser _user = Substitute.For<IUser>();
    private readonly PatchCapitalSetForSavingsCommandHandler _handler;

    public PatchCapitalSetForSavingsCommandHandlerTests()
    {
        _user.Name.Returns("testuser");
        _handler = new PatchCapitalSetForSavingsCommandHandler(_repository, _user);
    }

    [Fact]
    public async Task Handle_WhenUpdateSucceeds_ReturnsSuccess()
    {
        // Arrange
        PatchCapitalSetForSavingsCommand command = new(1, true);
        _repository.PatchSavingsOnlyAsync(1, true, "testuser", Arg.Any<CancellationToken>()).Returns(true);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUpdateFails_ReturnsFailure()
    {
        // Arrange
        PatchCapitalSetForSavingsCommand command = new(99, false);
        _repository.PatchSavingsOnlyAsync(99, false, "testuser", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.General.UpdateFailed);
    }
}
