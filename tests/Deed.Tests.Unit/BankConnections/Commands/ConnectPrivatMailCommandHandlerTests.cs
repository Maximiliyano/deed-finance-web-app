using Deed.Application.Abstractions.Settings;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Commands.ConnectPrivatMail;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class ConnectPrivatMailCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private const string Iban = "UA213223130000026007233566001";

    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IBankConnectionRepository _repositoryMock = Substitute.For<IBankConnectionRepository>();
    private readonly IDateTimeProvider _clockMock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    public ConnectPrivatMailCommandHandlerTests()
    {
        _userMock.Id.Returns("testuser");
        _clockMock.UtcNow.Returns(Now);
        _repositoryMock
            .GetAsync(Arg.Any<BankConnectionByAccountSpecification>(), Arg.Any<CancellationToken>())
            .Returns((BankConnection?)null);
    }

    [Fact]
    public async Task Handle_NewAccount_CreatesAnActiveConnectionWithoutAToken()
    {
        // Arrange
        var handler = CreateHandler(enabled: true);

        // Act
        var result = await handler.Handle(new ConnectPrivatMailCommand("ua21 3223 1300 0002 6007 2335 6600 1", null, " uah "), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MailKey.Should().MatchRegex("^[0-9A-F]{16}$");

        _repositoryMock.Received(1).Create(Arg.Is<BankConnection>(c =>
            c.Provider == BankProvider.PrivatMail &&
            c.AccountIban == Iban &&
            c.EncryptedToken == string.Empty &&
            c.ProviderAccountId == result.Value.MailKey &&
            c.Currency == "UAH" &&
            c.Status == BankConnectionStatus.Active &&
            c.LastSyncAt == Now &&
            c.Label == "Privat24 6001"));
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CustomLabel_IsKept()
    {
        // Arrange
        var handler = CreateHandler(enabled: true);

        // Act
        await handler.Handle(new ConnectPrivatMailCommand(Iban, " Salary card "), CancellationToken.None);

        // Assert
        _repositoryMock.Received(1).Create(Arg.Is<BankConnection>(c => c.Label == "Salary card" && c.Currency == null));
    }

    [Fact]
    public async Task Handle_AccountAlreadyConnected_ReturnsConflict()
    {
        // Arrange
        var handler = CreateHandler(enabled: true);
        _repositoryMock
            .GetAsync(Arg.Any<BankConnectionByAccountSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new BankConnection(1) { EncryptedToken = string.Empty, AccountIban = Iban });

        // Act
        var result = await handler.Handle(new ConnectPrivatMailCommand(Iban), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Bank.AccountAlreadyConnected);
        _repositoryMock.DidNotReceive().Create(Arg.Any<BankConnection>());
    }

    [Fact]
    public async Task Handle_MailboxNotConfigured_ReturnsError()
    {
        // Arrange
        var handler = CreateHandler(enabled: false);

        // Act
        var result = await handler.Handle(new ConnectPrivatMailCommand(Iban), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Bank.MailNotConfigured);
        await _repositoryMock.DidNotReceive().GetAsync(Arg.Any<BankConnectionByAccountSpecification>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private ConnectPrivatMailCommandHandler CreateHandler(bool enabled)
    {
        var settings = enabled
            ? new StatementMailSettings { Host = "imap.example.org", UserName = "u", Password = "p", Address = "deed.statements@gmail.com" }
            : new StatementMailSettings();

        return new ConnectPrivatMailCommandHandler(_userMock, _repositoryMock, Options.Create(settings), _clockMock, _unitOfWorkMock);
    }
}
