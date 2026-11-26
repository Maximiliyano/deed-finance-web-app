using Deed.Application.Auth;
using Deed.Application.BankConnections.Commands.ConnectBankAccount;
using Deed.Application.BankConnections.Service;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class ConnectBankAccountCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private const string Iban = "UA213223130000026007233566001";

    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IBankAccountRepository _accountsMock = Substitute.For<IBankAccountRepository>();
    private readonly IBankConnectionRepository _connectionsMock = Substitute.For<IBankConnectionRepository>();
    private readonly IDateTimeProvider _clockMock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IBankBackfillTrigger _triggerMock = Substitute.For<IBankBackfillTrigger>();

    private readonly ConnectBankAccountCommandHandler _handler;

    public ConnectBankAccountCommandHandlerTests()
    {
        _userMock.Name.Returns("owner");
        _clockMock.UtcNow.Returns(Now);

        _handler = new ConnectBankAccountCommandHandler(_userMock, _accountsMock, _connectionsMock, _clockMock, _unitOfWorkMock, _triggerMock);
    }

    [Fact]
    public async Task Handle_AvailableAccount_CreatesConnectionWithSourceToken()
    {
        // Arrange
        ReturnAccount(new BankAccount(5) { Iban = Iban, Name = "Main", Provider = BankProvider.PrivatAutoclient, SourceConnectionId = 1 });
        ReturnExisting(null);
        ReturnSource(new BankConnection(1) { EncryptedToken = "enc-token", AccountIban = "UA1", Label = "Business", Status = BankConnectionStatus.Active });

        // Act
        var result = await _handler.Handle(new ConnectBankAccountCommand(5, null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _connectionsMock.Received(1).Create(Arg.Is<BankConnection>(c =>
            c.EncryptedToken == "enc-token" &&
            c.AccountIban == Iban &&
            c.Label == "Main 6001" &&
            c.Status == BankConnectionStatus.Active &&
            c.Provider == BankProvider.PrivatAutoclient &&
            c.LastSyncAt == Now));
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CustomLabel_UsesIt()
    {
        // Arrange
        ReturnAccount(new BankAccount(5) { Iban = Iban, SourceConnectionId = 1 });
        ReturnExisting(null);
        ReturnSource(new BankConnection(1) { EncryptedToken = "enc", AccountIban = "UA1", Label = "Business", Status = BankConnectionStatus.Active });

        // Act
        await _handler.Handle(new ConnectBankAccountCommand(5, "  Savings  "), CancellationToken.None);

        // Assert
        _connectionsMock.Received(1).Create(Arg.Is<BankConnection>(c => c.Label == "Savings"));
    }

    [Fact]
    public async Task Handle_AvailableAccount_StartsFullHistoryBackfill()
    {
        // Arrange
        ReturnAccount(new BankAccount(5) { Iban = Iban, SourceConnectionId = 1 });
        ReturnExisting(null);
        ReturnSource(new BankConnection(1) { EncryptedToken = "enc", AccountIban = "UA1", Label = "Business", Status = BankConnectionStatus.Active });

        // Act
        await _handler.Handle(new ConnectBankAccountCommand(5, null), CancellationToken.None);

        // Assert
        _connectionsMock.Received(1).Create(Arg.Is<BankConnection>(c =>
            c.BackfillFrom == Now.AddDays(-3650) && c.BackfillCursor == Now));
    }

    [Fact]
    public async Task Handle_UnknownAccount_ReturnsNotFound()
    {
        // Arrange
        ReturnAccount(null);

        // Act
        var result = await _handler.Handle(new ConnectBankAccountCommand(5, null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Bank.AccountNotFound);
        _connectionsMock.DidNotReceive().Create(Arg.Any<BankConnection>());
    }

    [Fact]
    public async Task Handle_AlreadyConnected_ReturnsConflict()
    {
        // Arrange
        ReturnAccount(new BankAccount(5) { Iban = Iban, SourceConnectionId = 1 });
        ReturnExisting(new BankConnection(2) { EncryptedToken = "x", AccountIban = Iban });

        // Act
        var result = await _handler.Handle(new ConnectBankAccountCommand(5, null), CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.AccountAlreadyConnected);
        _connectionsMock.DidNotReceive().Create(Arg.Any<BankConnection>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_SourceMissingOrInvalid_ReturnsNotConnected(bool sourceExists)
    {
        // Arrange
        ReturnAccount(new BankAccount(5) { Iban = Iban, SourceConnectionId = 1 });
        ReturnExisting(null);
        ReturnSource(sourceExists
            ? new BankConnection(1) { EncryptedToken = "x", AccountIban = "UA1", Status = BankConnectionStatus.TokenInvalid }
            : null);

        // Act
        var result = await _handler.Handle(new ConnectBankAccountCommand(5, null), CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.NotConnected);
        _connectionsMock.DidNotReceive().Create(Arg.Any<BankConnection>());
    }

    private void ReturnAccount(BankAccount? account)
        => _accountsMock.GetAsync(Arg.Any<BankAccountByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(account);

    private void ReturnExisting(BankConnection? connection)
        => _connectionsMock.GetAsync(Arg.Any<BankConnectionByAccountSpecification>(), Arg.Any<CancellationToken>()).Returns(connection);

    private void ReturnSource(BankConnection? connection)
        => _connectionsMock.GetAsync(Arg.Any<BankConnectionByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(connection);
}
