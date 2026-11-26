using Deed.Application.Auth;
using Deed.Application.BankConnections.Commands.DisconnectBankConnection;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class DisconnectBankConnectionCommandHandlerTests
{
    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IBankConnectionRepository _repositoryMock = Substitute.For<IBankConnectionRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly DisconnectBankConnectionCommandHandler _handler;

    public DisconnectBankConnectionCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _handler = new DisconnectBankConnectionCommandHandler(_userMock, _repositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_ConnectionExists_DeletesIt()
    {
        // Arrange
        var connection = new BankConnection(1) { EncryptedToken = "enc", AccountIban = "UA1" };
        _repositoryMock.GetAsync(Arg.Any<BankConnectionByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(connection);

        // Act
        var result = await _handler.Handle(new DisconnectBankConnectionCommand(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Received(1).Delete(connection);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoConnection_ReturnsNotConnected()
    {
        // Arrange
        _repositoryMock.GetAsync(Arg.Any<BankConnectionByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((BankConnection?)null);

        // Act
        var result = await _handler.Handle(new DisconnectBankConnectionCommand(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Bank.NotConnected);

        _repositoryMock.DidNotReceive().Delete(Arg.Any<BankConnection>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
