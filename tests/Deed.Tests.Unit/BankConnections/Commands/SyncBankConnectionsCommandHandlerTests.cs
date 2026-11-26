using Deed.Application.Auth;
using Deed.Application.BankConnections.Commands.SyncBankConnections;
using Deed.Application.BankConnections.Service;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class SyncBankConnectionsCommandHandlerTests
{
    [Fact]
    public async Task Handle_SyncsOnlyTheCallersConnectionsAndReturnsTheCount()
    {
        // Arrange
        var user = Substitute.For<IUser>();
        user.Name.Returns("owner");
        var service = Substitute.For<IBankTransactionSyncService>();
        service.SyncUserAsync("owner", Arg.Any<CancellationToken>()).Returns(4);
        var handler = new SyncBankConnectionsCommandHandler(user, service);

        // Act
        var result = await handler.Handle(new SyncBankConnectionsCommand(), CancellationToken.None);

        // Assert
        result.Value.Should().Be(4);
        await service.DidNotReceive().SyncAsync(Arg.Any<CancellationToken>());
    }
}
