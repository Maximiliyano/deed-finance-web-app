using Deed.Application.Auth;
using Deed.Application.BankNotifications.Commands.MarkRead;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankNotifications;

public sealed class MarkBankNotificationsReadCommandHandlerTests
{
    private readonly IBankNotificationRepository _repositoryMock = Substitute.For<IBankNotificationRepository>();
    private readonly MarkBankNotificationsReadCommandHandler _handler;

    public MarkBankNotificationsReadCommandHandlerTests()
    {
        var user = Substitute.For<IUser>();
        user.Name.Returns("owner");

        _handler = new MarkBankNotificationsReadCommandHandler(user, _repositoryMock);
    }

    [Fact]
    public async Task Handle_SelectedIds_MarksOnlyThose()
    {
        // Arrange
        var command = new MarkBankNotificationsReadCommand([4, 5]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repositoryMock.Received(1).MarkReadAsync("owner", Arg.Is<IReadOnlyCollection<int>?>(ids => ids!.Count == 2), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public async Task Handle_NoIds_MarksEverythingUnread(int? count)
    {
        // Arrange
        IReadOnlyList<int>? ids = count is null ? null : new List<int>();
        var command = new MarkBankNotificationsReadCommand(ids);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repositoryMock.Received(1).MarkReadAsync("owner", null, Arg.Any<CancellationToken>());
    }
}
