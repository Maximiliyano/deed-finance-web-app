using Deed.Application.Auth;
using Deed.Application.BankConnections.Commands.DisconnectBankProvider;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class DisconnectBankProviderCommandHandlerTests
{
    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IBankConnectionRepository _connectionsMock = Substitute.For<IBankConnectionRepository>();
    private readonly IBankAccountRepository _accountsMock = Substitute.For<IBankAccountRepository>();
    private readonly IBankNotificationRepository _notificationsMock = Substitute.For<IBankNotificationRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly DisconnectBankProviderCommandHandler _handler;

    public DisconnectBankProviderCommandHandlerTests()
    {
        _userMock.Name.Returns("owner");

        _handler = new DisconnectBankProviderCommandHandler(_userMock, _connectionsMock, _accountsMock, _notificationsMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_Provider_RemovesItsConnectionsAccountsAndStoredTransactionsOnly()
    {
        // Arrange
        var privatOne = new BankConnection(1) { EncryptedToken = "x", AccountIban = "UA1", Provider = BankProvider.PrivatAutoclient };
        var privatTwo = new BankConnection(2) { EncryptedToken = "x", AccountIban = "UA2", Provider = BankProvider.PrivatAutoclient, IsExcluded = true };
        var mono = new BankConnection(3) { EncryptedToken = "x", AccountIban = "UA3", Provider = BankProvider.Monobank };
        _connectionsMock.GetAllAsync(Arg.Any<BankConnectionByUserSpecification>(), Arg.Any<CancellationToken>()).Returns([privatOne, privatTwo, mono]);
        var privatAccount = new BankAccount(10) { Iban = "UA9", Provider = BankProvider.PrivatAutoclient };
        var monoAccount = new BankAccount(11) { Iban = "UA8", Provider = BankProvider.Monobank };
        _accountsMock.GetAllAsync(Arg.Any<BankAccountsByUserSpecification>(), Arg.Any<CancellationToken>()).Returns([privatAccount, monoAccount]);

        // Act
        var result = await _handler.Handle(new DisconnectBankProviderCommand("PrivatAutoclient"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _connectionsMock.Received(1).Delete(privatOne);
        _connectionsMock.Received(1).Delete(privatTwo);
        _connectionsMock.DidNotReceive().Delete(mono);
        _accountsMock.Received(1).Delete(privatAccount);
        _accountsMock.DidNotReceive().Delete(monoAccount);
        await _notificationsMock.Received(1).DeleteByConnectionsAsync(
            "owner", Arg.Is<IReadOnlyCollection<int>>(ids => ids.Count == 2 && ids.Contains(1) && ids.Contains(2)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NothingConnected_ReturnsNotConnected()
    {
        // Arrange
        _connectionsMock.GetAllAsync(Arg.Any<BankConnectionByUserSpecification>(), Arg.Any<CancellationToken>()).Returns(new List<BankConnection>());
        _accountsMock.GetAllAsync(Arg.Any<BankAccountsByUserSpecification>(), Arg.Any<CancellationToken>()).Returns(new List<BankAccount>());

        // Act
        var result = await _handler.Handle(new DisconnectBankProviderCommand("Monobank"), CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.NotConnected);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
