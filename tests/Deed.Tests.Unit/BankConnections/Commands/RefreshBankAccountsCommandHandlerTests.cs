using Deed.Application.Auth;
using Deed.Application.BankConnections.Commands.RefreshBankAccounts;
using Deed.Application.BankConnections.Service;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class RefreshBankAccountsCommandHandlerTests
{
    [Fact]
    public async Task Handle_RefreshesOnlyTheCallersAccounts()
    {
        // Arrange
        var user = Substitute.For<IUser>();
        user.Name.Returns("owner");
        var service = Substitute.For<IBankAccountRefreshService>();
        service.RefreshUserAsync("owner", Arg.Any<CancellationToken>()).Returns(3);
        var handler = new RefreshBankAccountsCommandHandler(user, service);

        // Act
        var result = await handler.Handle(new RefreshBankAccountsCommand(), CancellationToken.None);

        // Assert
        result.Value.Should().Be(3);
        await service.DidNotReceive().RefreshAsync(Arg.Any<CancellationToken>());
    }
}
