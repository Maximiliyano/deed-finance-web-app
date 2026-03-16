using Deed.Application.Auth;
using Deed.Application.Capitals.Commands.PatchIncludeInTotal;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.Capitals.Commands;

public sealed class PatchCapitalIncludeInTotalCommandHandlerTests
{
    private readonly ICapitalRepository _repository = Substitute.For<ICapitalRepository>();
    private readonly IUser _user = Substitute.For<IUser>();
    private readonly PatchCapitalIncludeInTotalCommandHandler _handler;

    public PatchCapitalIncludeInTotalCommandHandlerTests()
    {
        _user.Name.Returns("testuser");
        _handler = new PatchCapitalIncludeInTotalCommandHandler(_repository, _user);
    }

    [Fact]
    public async Task Handle_WhenUpdateSucceeds_ReturnsSuccess()
    {
        // Arrange
        PatchCapitalIncludeInTotalCommand command = new(1, false);
        _repository.PatchIncludeInTotalAsync(1, false, "testuser", Arg.Any<CancellationToken>()).Returns(true);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUpdateFails_ReturnsFailure()
    {
        // Arrange
        PatchCapitalIncludeInTotalCommand command = new(99, true);
        _repository.PatchIncludeInTotalAsync(99, true, "testuser", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.General.UpdateFailed);
    }
}
