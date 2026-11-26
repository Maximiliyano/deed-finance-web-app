using Deed.Application.Abstractions.Services;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Commands.ConnectPrivat;
using Deed.Application.BankConnections.Service;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankConnections.Commands;

public sealed class ConnectPrivatCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private const string Iban = "UA213223130000026007233566001";

    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IBankConnectionRepository _repositoryMock = Substitute.For<IBankConnectionRepository>();
    private readonly IPrivatAutoclientService _bankMock = Substitute.For<IPrivatAutoclientService>();
    private readonly ITokenProtector _protectorMock = Substitute.For<ITokenProtector>();
    private readonly IDateTimeProvider _clockMock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly ConnectPrivatCommandHandler _handler;

    public ConnectPrivatCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _clockMock.UtcNow.Returns(Now);
        _protectorMock.Protect(Arg.Any<string>()).Returns(call => $"enc:{call.Arg<string>()}");

        _handler = new ConnectPrivatCommandHandler(
            _userMock, _repositoryMock, _bankMock, _protectorMock, _clockMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_ValidToken_CreatesEncryptedConnection()
    {
        // Arrange
        ReturnBankSuccess();
        _repositoryMock.GetAsync(Arg.Any<BankConnectionByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns((BankConnection?)null);

        // Act
        var result = await _handler.Handle(new ConnectPrivatCommand(" secret ", "ua21 3223 1300 0002 6007 2335 6600 1"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Received(1).Create(Arg.Is<BankConnection>(c =>
            c.EncryptedToken == "enc:secret" &&
            c.AccountIban == Iban &&
            c.Status == BankConnectionStatus.Active &&
            c.Provider == BankProvider.PrivatAutoclient &&
            c.LastSyncAt == Now));
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoIban_StoresConnectionForAllAccounts()
    {
        // Arrange
        ReturnBankSuccess();
        _repositoryMock.GetAsync(Arg.Any<BankConnectionByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns((BankConnection?)null);

        // Act
        var result = await _handler.Handle(new ConnectPrivatCommand("secret", null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _bankMock.Received(1).GetTransactionsAsync(
            "secret", string.Empty, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
        _repositoryMock.Received(1).Create(Arg.Is<BankConnection>(c => c.AccountIban == string.Empty));
    }

    [Fact]
    public async Task Handle_InvalidToken_ReturnsFailureAndDoesNotSave()
    {
        // Arrange
        _bankMock
            .GetTransactionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.Bank.TokenInvalid));

        // Act
        var result = await _handler.Handle(new ConnectPrivatCommand("bad", Iban), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Bank.TokenInvalid);

        _repositoryMock.DidNotReceive().Create(Arg.Any<BankConnection>());
        _protectorMock.DidNotReceive().Protect(Arg.Any<string>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingConnection_ReactivatesAndUpdatesToken()
    {
        // Arrange
        ReturnBankSuccess();
        var existing = new BankConnection(5)
        {
            EncryptedToken = "old",
            AccountIban = "UA000000000000000000000000000",
            Status = BankConnectionStatus.TokenInvalid,
            LastSyncAt = Now.AddDays(-10)
        };
        _repositoryMock.GetAsync(Arg.Any<BankConnectionByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(existing);

        // Act
        var result = await _handler.Handle(new ConnectPrivatCommand("fresh", Iban), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existing.EncryptedToken.Should().Be("enc:fresh");
        existing.AccountIban.Should().Be(Iban);
        existing.Status.Should().Be(BankConnectionStatus.Active);
        existing.LastSyncAt.Should().Be(Now);

        _repositoryMock.DidNotReceive().Create(Arg.Any<BankConnection>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private void ReturnBankSuccess()
        => _bankMock
            .GetTransactionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<BankTransaction>>([]));
}
