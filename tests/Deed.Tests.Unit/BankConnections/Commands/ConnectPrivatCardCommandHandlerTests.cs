using Deed.Application.Abstractions.Services;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Commands.ConnectPrivatCard;
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

public sealed class ConnectPrivatCardCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private const string Card = "4111111111111111";

    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IBankConnectionRepository _repositoryMock = Substitute.For<IBankConnectionRepository>();
    private readonly IPrivatCardService _bankMock = Substitute.For<IPrivatCardService>();
    private readonly ITokenProtector _protectorMock = Substitute.For<ITokenProtector>();
    private readonly IDateTimeProvider _clockMock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly ConnectPrivatCardCommandHandler _handler;

    public ConnectPrivatCardCommandHandlerTests()
    {
        _userMock.Name.Returns("testuser");
        _clockMock.UtcNow.Returns(Now);
        _protectorMock.Protect(Arg.Any<string>()).Returns(call => $"enc:{call.Arg<string>()}");

        _handler = new ConnectPrivatCardCommandHandler(
            _userMock, _repositoryMock, _bankMock, _protectorMock, _clockMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_ValidCredentials_CreatesEncryptedCardConnection()
    {
        // Arrange
        ReturnBankSuccess();
        _repositoryMock.GetAsync(Arg.Any<BankConnectionByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns((BankConnection?)null);

        var expected = $"enc:{new PrivatCardCredentials("12345", "pass").Serialize()}";

        // Act
        var result = await _handler.Handle(new ConnectPrivatCardCommand(" 12345 ", "pass", "4111 1111 1111 1111"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Received(1).Create(Arg.Is<BankConnection>(c =>
            c.Provider == BankProvider.PrivatCard &&
            c.AccountIban == Card &&
            c.EncryptedToken == expected &&
            c.Status == BankConnectionStatus.Active &&
            c.LastSyncAt == Now));
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BankRejects_ReturnsFailureAndDoesNotSave()
    {
        // Arrange
        _bankMock
            .GetTransactionsAsync(Arg.Any<PrivatCardCredentials>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.Bank.TokenInvalid));

        // Act
        var result = await _handler.Handle(new ConnectPrivatCardCommand("12345", "bad", Card), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(DomainErrors.Bank.TokenInvalid);

        _repositoryMock.DidNotReceive().Create(Arg.Any<BankConnection>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingAutoclientConnection_IsReplacedByCard()
    {
        // Arrange
        ReturnBankSuccess();
        var existing = new BankConnection(5)
        {
            Provider = BankProvider.PrivatAutoclient,
            EncryptedToken = "old",
            AccountIban = "UA213223130000026007233566001",
            Status = BankConnectionStatus.TokenInvalid,
            LastSyncAt = Now.AddDays(-10)
        };
        _repositoryMock.GetAsync(Arg.Any<BankConnectionByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(existing);

        // Act
        var result = await _handler.Handle(new ConnectPrivatCardCommand("12345", "pass", Card), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existing.Provider.Should().Be(BankProvider.PrivatCard);
        existing.AccountIban.Should().Be(Card);
        existing.Status.Should().Be(BankConnectionStatus.Active);
        existing.LastSyncAt.Should().Be(Now);

        _repositoryMock.DidNotReceive().Create(Arg.Any<BankConnection>());
    }

    private void ReturnBankSuccess()
        => _bankMock
            .GetTransactionsAsync(Arg.Any<PrivatCardCredentials>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<BankTransaction>>([]));
}
