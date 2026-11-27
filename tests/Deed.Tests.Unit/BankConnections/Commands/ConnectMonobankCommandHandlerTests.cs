using Deed.Application.Abstractions.Services;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Commands.ConnectMonobank;
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

public sealed class ConnectMonobankCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private const string Iban = "UA213223130000026007233566001";

    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IBankConnectionRepository _repositoryMock = Substitute.For<IBankConnectionRepository>();
    private readonly IMonobankService _bankMock = Substitute.For<IMonobankService>();
    private readonly IBankTransactionSyncService _syncMock = Substitute.For<IBankTransactionSyncService>();
    private readonly ITokenProtector _protectorMock = Substitute.For<ITokenProtector>();
    private readonly IDateTimeProvider _clockMock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IBankBackfillTrigger _triggerMock = Substitute.For<IBankBackfillTrigger>();

    private readonly ConnectMonobankCommandHandler _handler;

    public ConnectMonobankCommandHandlerTests()
    {
        _userMock.Id.Returns("owner");
        _clockMock.UtcNow.Returns(Now);
        _protectorMock.Protect(Arg.Any<string>()).Returns(call => $"enc:{call.Arg<string>()}");
        _repositoryMock
            .GetAsync(Arg.Any<BankConnectionByAccountSpecification>(), Arg.Any<CancellationToken>())
            .Returns((BankConnection?)null);

        _handler = new ConnectMonobankCommandHandler(
            _userMock, _repositoryMock, _bankMock, _syncMock, _protectorMock, _clockMock, _unitOfWorkMock, _triggerMock);
    }

    [Fact]
    public async Task Handle_ValidToken_CreatesEncryptedConnectionAndStartsHistory()
    {
        // Arrange
        ReturnHistory();

        // Act
        var result = await _handler.Handle(new ConnectMonobankCommand(" secret ", "acc-1", Iban, "Black"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Received(1).Create(Arg.Is<BankConnection>(c =>
            c.Provider == BankProvider.Monobank &&
            c.EncryptedToken == "enc:secret" &&
            c.ProviderAccountId == "acc-1" &&
            c.AccountIban == Iban &&
            c.Label == "Black" &&
            c.Status == BankConnectionStatus.Active &&
            c.BackfillFrom == Now.AddDays(-3650) &&
            c.BackfillCursor == Now.AddDays(-30)));
        await _triggerMock.Received(1).TriggerAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RateLimitedCheck_StillConnectsAndStartsHistoryFromNow()
    {
        // Arrange
        _bankMock
            .GetTransactionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.Bank.RateLimited));

        // Act
        var result = await _handler.Handle(new ConnectMonobankCommand("secret", "acc-1", Iban), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Received(1).Create(Arg.Is<BankConnection>(c => c.BackfillCursor == Now));
    }

    [Fact]
    public async Task Handle_InvalidToken_FailsWithoutSaving()
    {
        // Arrange
        _bankMock
            .GetTransactionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.Bank.TokenInvalid));

        // Act
        var result = await _handler.Handle(new ConnectMonobankCommand("bad", "acc-1", Iban), CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.TokenInvalid);
        _repositoryMock.DidNotReceive().Create(Arg.Any<BankConnection>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoIban_UsesTheAccountIdAsTheAccountKey()
    {
        // Arrange
        ReturnHistory();

        // Act
        await _handler.Handle(new ConnectMonobankCommand("secret", "acc-1", null), CancellationToken.None);

        // Assert
        _repositoryMock.Received(1).Create(Arg.Is<BankConnection>(c => c.AccountIban == "acc-1"));
    }

    [Fact]
    public async Task Handle_ExistingConnection_ReactivatesItWithTheNewToken()
    {
        // Arrange
        ReturnHistory();
        var existing = new BankConnection(5) { EncryptedToken = "old", AccountIban = Iban, Label = "Keep", Status = BankConnectionStatus.TokenInvalid };
        _repositoryMock
            .GetAsync(Arg.Any<BankConnectionByAccountSpecification>(), Arg.Any<CancellationToken>())
            .Returns(existing);

        // Act
        await _handler.Handle(new ConnectMonobankCommand("fresh", "acc-1", Iban), CancellationToken.None);

        // Assert
        existing.EncryptedToken.Should().Be("enc:fresh");
        existing.Status.Should().Be(BankConnectionStatus.Active);
        existing.Label.Should().Be("Keep");
        _repositoryMock.DidNotReceive().Create(Arg.Any<BankConnection>());
    }

    private void ReturnHistory()
        => _bankMock
            .GetTransactionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<BankTransaction>>([]));

    [Fact]
    public async Task Handle_AccountCurrencyGiven_IsStoredOnTheConnectionAndUsedForHistory()
    {
        // Arrange
        ReturnHistory();

        // Act
        await _handler.Handle(new ConnectMonobankCommand("secret", "acc-1", Iban, null, " uah "), CancellationToken.None);

        // Assert
        _repositoryMock.Received(1).Create(Arg.Is<BankConnection>(c => c.Currency == "UAH"));
        await _syncMock.Received(1).StageHistoryAsync("owner", Arg.Any<string>(), Arg.Any<int?>(), "UAH", Arg.Any<IReadOnlyCollection<BankTransaction>>(), Arg.Any<CancellationToken>());
    }
}
