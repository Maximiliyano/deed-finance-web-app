using Deed.Application.Abstractions.Services;
using Deed.Application.BankConnections.Service;
using Deed.Application.BankNotifications.Responses;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.BankNotifications.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using UserSettingsEntity = Deed.Domain.Entities.UserSettings;

namespace Deed.Tests.Unit.BankConnections.Services;

public sealed class BankTransactionSyncServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly IBankConnectionRepository _connectionsMock = Substitute.For<IBankConnectionRepository>();
    private readonly IBankNotificationRepository _notificationsMock = Substitute.For<IBankNotificationRepository>();
    private readonly IUserSettingsRepository _settingsMock = Substitute.For<IUserSettingsRepository>();
    private readonly IPrivatAutoclientService _bankMock = Substitute.For<IPrivatAutoclientService>();
    private readonly ITokenProtector _protectorMock = Substitute.For<ITokenProtector>();
    private readonly IEmailService _emailMock = Substitute.For<IEmailService>();
    private readonly IBankNotificationPublisher _publisherMock = Substitute.For<IBankNotificationPublisher>();
    private readonly IDateTimeProvider _clockMock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly BankTransactionSyncService _service;
    private readonly BankConnection _connection;

    public BankTransactionSyncServiceTests()
    {
        _clockMock.UtcNow.Returns(Now);
        _protectorMock.Unprotect("enc").Returns("token");

        _connection = new BankConnection(1)
        {
            EncryptedToken = "enc",
            AccountIban = "UA213223130000026007233566001",
            Status = BankConnectionStatus.Active,
            LastSyncAt = Now.AddMinutes(-5),
            CreatedAt = Now.AddMinutes(-30),
            CreatedBy = "owner"
        };

        _connectionsMock
            .GetAllAsync(Arg.Any<ActiveBankConnectionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([_connection]);

        _notificationsMock
            .GetAllAsync(Arg.Any<BankNotificationsByExternalIdsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([]);

        _service = new BankTransactionSyncService(
            _connectionsMock, _notificationsMock, _settingsMock, _bankMock,
            _protectorMock, _emailMock, _publisherMock, _clockMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task SyncAsync_NewTransaction_CreatesNotificationOwnedByUser()
    {
        // Arrange
        ReturnTransactions(Transaction("tx-1", Now.AddMinutes(-2)));

        // Act
        await _service.SyncAsync(CancellationToken.None);

        // Assert
        _notificationsMock.Received(1).CreateRange(Arg.Is<IEnumerable<BankNotification>>(items =>
            items.Count() == 1 &&
            items.All(n => n.CreatedBy == "owner" &&
                           n.ExternalId == "tx-1" &&
                           n.Kind == BankNotificationKind.Transaction &&
                           n.Direction == BankTransactionDirection.Outgoing &&
                           n.Amount == 150m)));
        _connection.LastSyncAt.Should().Be(Now);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncAsync_Withdrawal_IsClassifiedAndLabelledWithAccount()
    {
        // Arrange
        _connection.Label = "Personal";
        ReturnTransactions(new BankTransaction(
            "tx-w", BankTransactionDirection.Outgoing, 500m, "UAH", "ATM 12", "Зняття готівки", Now.AddMinutes(-2)));

        // Act
        await _service.SyncAsync(CancellationToken.None);

        // Assert
        _notificationsMock.Received(1).CreateRange(Arg.Is<IEnumerable<BankNotification>>(items =>
            items.Single().Type == BankTransactionType.Withdrawal &&
            items.Single().AccountLabel == "Personal"));
    }

    [Fact]
    public async Task SyncAsync_NewTransaction_PushesNotificationToOwner()
    {
        // Arrange
        ReturnTransactions(Transaction("tx-1", Now.AddMinutes(-2)));

        // Act
        var created = await _service.SyncAsync(CancellationToken.None);

        // Assert
        created.Should().Be(1);
        await _publisherMock.Received(1).PublishAsync(
            "owner",
            Arg.Is<IReadOnlyList<BankNotificationResponse>>(items => items.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncAsync_PublisherFails_StillSavesNotifications()
    {
        // Arrange
        ReturnTransactions(Transaction("tx-1", Now.AddMinutes(-2)));
        _publisherMock
            .PublishAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<BankNotificationResponse>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("hub down"));

        // Act
        var created = await _service.SyncAsync(CancellationToken.None);

        // Assert
        created.Should().Be(1);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncAsync_LatePostedTransactionAfterLastSync_IsStillNotified()
    {
        // Arrange
        ReturnTransactions(Transaction("late", Now.AddMinutes(-20)));

        // Act
        var created = await _service.SyncAsync(CancellationToken.None);

        // Assert
        created.Should().Be(1);
    }

    [Fact]
    public async Task SyncUserAsync_NoTransactions_ReturnsZero()
    {
        // Arrange
        ReturnTransactions();

        // Act
        var created = await _service.SyncUserAsync("owner", CancellationToken.None);

        // Assert
        created.Should().Be(0);
        await _publisherMock.DidNotReceive().PublishAsync(
            Arg.Any<string>(), Arg.Any<IReadOnlyList<BankNotificationResponse>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncAsync_AlreadyNotifiedTransaction_IsSkipped()
    {
        // Arrange
        ReturnTransactions(Transaction("tx-1", Now.AddMinutes(-2)));
        _notificationsMock
            .GetAllAsync(Arg.Any<BankNotificationsByExternalIdsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([new BankNotification { ExternalId = "tx-1" }]);

        // Act
        await _service.SyncAsync(CancellationToken.None);

        // Assert
        _notificationsMock.DidNotReceive().CreateRange(Arg.Any<IEnumerable<BankNotification>>());
        _connection.LastSyncAt.Should().Be(Now);
    }

    [Fact]
    public async Task SyncAsync_TransactionBeforeConnectionWasCreated_IsIgnored()
    {
        // Arrange
        ReturnTransactions(Transaction("old", Now.AddHours(-6)));

        // Act
        await _service.SyncAsync(CancellationToken.None);

        // Assert
        _notificationsMock.DidNotReceive().CreateRange(Arg.Any<IEnumerable<BankNotification>>());
    }

    [Fact]
    public async Task SyncAsync_TokenRejected_MarksConnectionAndCreatesIssueNotification()
    {
        // Arrange
        _bankMock
            .GetTransactionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.Bank.TokenInvalid));

        // Act
        await _service.SyncAsync(CancellationToken.None);

        // Assert
        _connection.Status.Should().Be(BankConnectionStatus.TokenInvalid);
        _notificationsMock.Received(1).CreateRange(Arg.Is<IEnumerable<BankNotification>>(items =>
            items.Single().Kind == BankNotificationKind.ConnectionIssue &&
            items.Single().CreatedBy == "owner"));
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncAsync_TransientFailure_LeavesStateUntouched()
    {
        // Arrange
        _bankMock
            .GetTransactionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.HttpClient.Execution));

        // Act
        await _service.SyncAsync(CancellationToken.None);

        // Assert
        _connection.Status.Should().Be(BankConnectionStatus.Active);
        _connection.LastSyncAt.Should().Be(Now.AddMinutes(-5));
        _notificationsMock.DidNotReceive().CreateRange(Arg.Any<IEnumerable<BankNotification>>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncAsync_UnreadableToken_MarksConnectionInvalidWithoutCallingBank()
    {
        // Arrange
        _protectorMock.Unprotect("enc").Returns((string?)null);

        // Act
        await _service.SyncAsync(CancellationToken.None);

        // Assert
        _connection.Status.Should().Be(BankConnectionStatus.TokenInvalid);
        await _bankMock.DidNotReceive().GetTransactionsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncAsync_BankAlertsEnabled_SendsEmail()
    {
        // Arrange
        ReturnTransactions(Transaction("tx-1", Now.AddMinutes(-2)));
        _settingsMock.GetAsync("owner", Arg.Any<CancellationToken>()).Returns(new UserSettingsEntity
        {
            BankAlertsEmailEnabled = true,
            EmailNotificationsEnabled = true,
            Email = "owner@example.com"
        });

        // Act
        await _service.SyncAsync(CancellationToken.None);

        // Assert
        await _emailMock.Received(1).SendAsync("owner@example.com", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncAsync_BankAlertsDisabled_DoesNotSendEmail()
    {
        // Arrange
        ReturnTransactions(Transaction("tx-1", Now.AddMinutes(-2)));
        _settingsMock.GetAsync("owner", Arg.Any<CancellationToken>()).Returns(new UserSettingsEntity
        {
            BankAlertsEmailEnabled = false,
            EmailNotificationsEnabled = true,
            Email = "owner@example.com"
        });

        // Act
        await _service.SyncAsync(CancellationToken.None);

        // Assert
        await _emailMock.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StageHistoryAsync_NewAndKnownTransactions_StoresOnlyNewAsRead()
    {
        // Arrange
        _notificationsMock
            .GetAllAsync(Arg.Any<BankNotificationsByExternalIdsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([new BankNotification { ExternalId = "old" }]);

        // Act
        var staged = await _service.StageHistoryAsync(
            "owner", "Biz", [Transaction("old", Now.AddDays(-9)), Transaction("new", Now.AddDays(-8)), Transaction("new", Now.AddDays(-8))], CancellationToken.None);

        // Assert
        staged.Should().Be(1);
        _notificationsMock.Received(1).CreateRange(Arg.Is<IEnumerable<BankNotification>>(items =>
            items.Count() == 1 &&
            items.Single().ExternalId == "new" &&
            items.Single().IsRead &&
            items.Single().CreatedBy == "owner" &&
            items.Single().AccountLabel == "Biz"));
        await _publisherMock.DidNotReceive().PublishAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<BankNotificationResponse>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportHistoryAsync_ActiveConnection_FetchesThirtyDaysAndStoresAsRead()
    {
        // Arrange
        ReturnTransactions(Transaction("tx-old", Now.AddDays(-20)));

        // Act
        var imported = await _service.ImportHistoryAsync("owner", CancellationToken.None);

        // Assert
        imported.Should().Be(1);
        await _bankMock.Received(1).GetTransactionsAsync("token", _connection.AccountIban, Now.AddDays(-30), Now, Arg.Any<CancellationToken>());
        _notificationsMock.Received(1).CreateRange(Arg.Is<IEnumerable<BankNotification>>(items => items.Single().IsRead));
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static BankTransaction Transaction(string id, DateTimeOffset occurredAt)
        => new(id, BankTransactionDirection.Outgoing, 150m, "UAH", "John", "Rent", occurredAt);

    private void ReturnTransactions(params BankTransaction[] transactions)
        => _bankMock
            .GetTransactionsAsync("token", _connection.AccountIban, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<BankTransaction>>(transactions));
}
