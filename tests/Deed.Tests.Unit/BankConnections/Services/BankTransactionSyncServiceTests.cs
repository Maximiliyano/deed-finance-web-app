using Deed.Application.Abstractions.Services;
using Deed.Application.BankConnections.Service;
using Deed.Application.BankNotifications.Import;
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
    private readonly IBankTransactionImporter _importerMock = Substitute.For<IBankTransactionImporter>();
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
        _bankMock.Provider.Returns(BankProvider.PrivatAutoclient);
        _protectorMock.Unprotect("enc").Returns("token");

        _connection = new BankConnection(1)
        {
            Provider = BankProvider.PrivatAutoclient,
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
            _connectionsMock, _notificationsMock, _settingsMock, [_bankMock], _importerMock,
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
            "owner", "Biz", 7, [Transaction("old", Now.AddDays(-9)), Transaction("new", Now.AddDays(-8)), Transaction("new", Now.AddDays(-8))], CancellationToken.None);

        // Assert
        staged.Should().Be(1);
        _notificationsMock.Received(1).CreateRange(Arg.Is<IEnumerable<BankNotification>>(items =>
            items.Count() == 1 &&
            items.Single().ExternalId == "new" &&
            items.Single().IsRead &&
            items.Single().CreatedBy == "owner" &&
            items.Single().AccountLabel == "Biz" &&
            items.Single().BankConnectionId == 7));
        await _publisherMock.DidNotReceive().PublishAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<BankNotificationResponse>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportHistoryAsync_ActiveConnection_SchedulesBackfillWithoutCallingBank()
    {
        // Arrange
        var from = Now.AddDays(-90);

        // Act
        var response = await _service.ImportHistoryAsync("owner", from, Now, CancellationToken.None);

        // Assert
        response.Imported.Should().Be(0);
        response.InProgress.Should().BeTrue();
        _connection.BackfillFrom.Should().Be(from);
        _connection.BackfillCursor.Should().Be(Now);
        await _bankMock.DidNotReceive().GetTransactionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BackfillAsync_FirstFailure_KeepsBackfillAndCountsTheFailure()
    {
        // Arrange
        _connection.BackfillFrom = Now.AddDays(-90);
        _connection.BackfillCursor = Now;
        _connectionsMock
            .GetAllAsync(Arg.Any<BackfillBankConnectionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([_connection]);
        _bankMock
            .GetTransactionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.HttpClient.Execution));

        // Act
        await _service.BackfillAsync(CancellationToken.None);

        // Assert
        _connection.BackfillCursor.Should().Be(Now);
        _connection.BackfillFailures.Should().Be(1);
    }

    [Fact]
    public async Task BackfillAsync_RateLimited_DoesNotCountAsFailure()
    {
        // Arrange
        _connection.BackfillFrom = Now.AddDays(-90);
        _connection.BackfillCursor = Now;
        _connectionsMock
            .GetAllAsync(Arg.Any<BackfillBankConnectionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([_connection]);
        _bankMock
            .GetTransactionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.Bank.RateLimited));

        // Act
        await _service.BackfillAsync(CancellationToken.None);

        // Assert
        _connection.BackfillCursor.Should().Be(Now);
        _connection.BackfillFailures.Should().Be(0);
    }

    [Fact]
    public async Task BackfillAsync_SuccessAfterFailures_ResetsTheCounter()
    {
        // Arrange
        _connection.BackfillFrom = Now.AddDays(-90);
        _connection.BackfillCursor = Now;
        _connection.BackfillFailures = 4;
        _connectionsMock
            .GetAllAsync(Arg.Any<BackfillBankConnectionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([_connection]);
        ReturnTransactions(Transaction("tx", Now.AddDays(-5)));

        // Act
        await _service.BackfillAsync(CancellationToken.None);

        // Assert
        _connection.BackfillFailures.Should().Be(0);
    }

    [Fact]
    public async Task BackfillAsync_TenthFailure_StopsBackfill()
    {
        // Arrange
        _connection.BackfillFrom = Now.AddDays(-90);
        _connection.BackfillCursor = Now;
        _connection.BackfillFailures = 9;
        _connectionsMock
            .GetAllAsync(Arg.Any<BackfillBankConnectionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([_connection]);
        _bankMock
            .GetTransactionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.HttpClient.Execution));

        // Act
        var imported = await _service.BackfillAsync(CancellationToken.None);

        // Assert
        imported.Should().Be(0);
        _connection.BackfillFrom.Should().BeNull();
        _connection.BackfillCursor.Should().BeNull();
    }

    [Fact]
    public async Task BackfillAsync_ConnectionInProgress_FetchesNextChunkBackwards()
    {
        // Arrange
        _connection.BackfillFrom = Now.AddDays(-90);
        _connection.BackfillCursor = Now.AddDays(-30);
        _connectionsMock
            .GetAllAsync(Arg.Any<BackfillBankConnectionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([_connection]);
        ReturnTransactions(Transaction("tx-mid", Now.AddDays(-45)));

        // Act
        var imported = await _service.BackfillAsync(CancellationToken.None);

        // Assert
        imported.Should().Be(1);
        _connection.BackfillCursor.Should().Be(Now.AddDays(-60));
        await _bankMock.Received(1).GetTransactionsAsync("token", _connection.AccountIban, Now.AddDays(-60), Now.AddDays(-30), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BackfillAsync_LastChunk_ClearsBackfillState()
    {
        // Arrange
        _connection.BackfillFrom = Now.AddDays(-40);
        _connection.BackfillCursor = Now.AddDays(-30);
        _connectionsMock
            .GetAllAsync(Arg.Any<BackfillBankConnectionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([_connection]);
        ReturnTransactions();

        // Act
        await _service.BackfillAsync(CancellationToken.None);

        // Assert
        _connection.BackfillFrom.Should().BeNull();
        _connection.BackfillCursor.Should().BeNull();
        await _bankMock.Received(1).GetTransactionsAsync("token", _connection.AccountIban, Now.AddDays(-40), Now.AddDays(-30), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BackfillAsync_ProviderAllowsSeveralChunks_WalksThemInOneRun()
    {
        // Arrange
        _bankMock.MaxChunksPerRun.Returns(3);
        _connection.BackfillFrom = Now.AddDays(-365);
        _connection.BackfillCursor = Now;
        _connectionsMock
            .GetAllAsync(Arg.Any<BackfillBankConnectionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([_connection]);
        ReturnTransactions(Transaction("tx", Now.AddDays(-5)));

        // Act
        await _service.BackfillAsync(CancellationToken.None);

        // Assert
        _connection.BackfillCursor.Should().Be(Now.AddDays(-90));
        await _bankMock.Received(3).GetTransactionsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BackfillAsync_RateLimitedInTheMiddle_StopsTheRun()
    {
        // Arrange
        _bankMock.MaxChunksPerRun.Returns(5);
        _connection.BackfillFrom = Now.AddDays(-365);
        _connection.BackfillCursor = Now;
        _connectionsMock
            .GetAllAsync(Arg.Any<BackfillBankConnectionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([_connection]);
        _bankMock
            .GetTransactionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(
                Result.Success<IReadOnlyList<BankTransaction>>([Transaction("tx", Now.AddDays(-5))]),
                Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.Bank.RateLimited));

        // Act
        await _service.BackfillAsync(CancellationToken.None);

        // Assert
        _connection.BackfillCursor.Should().Be(Now.AddDays(-30));
        await _bankMock.Received(2).GetTransactionsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BackfillAsync_EmptyChunkLongBeforeOldestKnown_StopsBackfill()
    {
        // Arrange
        _connection.BackfillFrom = Now.AddYears(-10);
        _connection.BackfillCursor = Now.AddDays(-1000);
        _connectionsMock
            .GetAllAsync(Arg.Any<BackfillBankConnectionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([_connection]);
        _notificationsMock
            .GetOldestAsync(Arg.Any<BankTransactionsSummarySpecification>(), Arg.Any<CancellationToken>())
            .Returns((DateTimeOffset?)Now.AddDays(-200));
        ReturnTransactions();

        // Act
        await _service.BackfillAsync(CancellationToken.None);

        // Assert
        _connection.BackfillFrom.Should().BeNull();
        _connection.BackfillCursor.Should().BeNull();
    }

    [Fact]
    public async Task BackfillAsync_EmptyChunkCloseToOldestKnown_KeepsGoing()
    {
        // Arrange
        _connection.BackfillFrom = Now.AddYears(-10);
        _connection.BackfillCursor = Now.AddDays(-300);
        _connectionsMock
            .GetAllAsync(Arg.Any<BackfillBankConnectionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([_connection]);
        _notificationsMock
            .GetOldestAsync(Arg.Any<BankTransactionsSummarySpecification>(), Arg.Any<CancellationToken>())
            .Returns((DateTimeOffset?)Now.AddDays(-200));
        ReturnTransactions();

        // Act
        await _service.BackfillAsync(CancellationToken.None);

        // Assert
        _connection.BackfillCursor.Should().Be(Now.AddDays(-330));
    }

    [Fact]
    public async Task BackfillAsync_ProviderReturnsFullPage_ResumesFromOldestTransaction()
    {
        // Arrange
        var monobank = Substitute.For<IBankStatementProvider>();
        monobank.Provider.Returns(BankProvider.Monobank);
        monobank.MaxItemsPerRequest.Returns(2);
        monobank
            .GetTransactionsAsync("token", "acc-id", Now.AddDays(-30), Now, Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<BankTransaction>>([Transaction("m1", Now.AddDays(-2)), Transaction("m2", Now.AddDays(-12))]));
        _connection.Provider = BankProvider.Monobank;
        _connection.ProviderAccountId = "acc-id";
        _connection.BackfillFrom = Now.AddDays(-90);
        _connection.BackfillCursor = Now;
        _connectionsMock
            .GetAllAsync(Arg.Any<BackfillBankConnectionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([_connection]);
        var service = new BankTransactionSyncService(
            _connectionsMock, _notificationsMock, _settingsMock, [monobank], _importerMock,
            _protectorMock, _emailMock, _publisherMock, _clockMock, _unitOfWorkMock);

        // Act
        await service.BackfillAsync(CancellationToken.None);

        // Assert
        _connection.BackfillCursor.Should().Be(Now.AddDays(-12));
    }

    [Fact]
    public async Task BackfillAsync_TokenRejected_StopsBackfillAndMarksConnectionInvalid()
    {
        // Arrange
        _connection.BackfillFrom = Now.AddDays(-90);
        _connection.BackfillCursor = Now;
        _connectionsMock
            .GetAllAsync(Arg.Any<BackfillBankConnectionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([_connection]);
        _bankMock
            .GetTransactionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<BankTransaction>>(DomainErrors.Bank.TokenInvalid));

        // Act
        await _service.BackfillAsync(CancellationToken.None);

        // Assert
        _connection.Status.Should().Be(BankConnectionStatus.TokenInvalid);
        _connection.BackfillCursor.Should().BeNull();
    }

    [Fact]
    public async Task SyncAsync_ConnectionWithCapital_MarksNewTransactionsPending()
    {
        // Arrange
        _connection.CapitalId = 3;
        ReturnTransactions(Transaction("tx-1", Now.AddMinutes(-2)));

        // Act
        await _service.SyncAsync(CancellationToken.None);

        // Assert
        _notificationsMock.Received(1).CreateRange(Arg.Is<IEnumerable<BankNotification>>(items =>
            items.Single().ImportStatus == BankImportStatus.Pending && items.Single().BankConnectionId == 1));
        await _importerMock.DidNotReceive().ImportAsync(
            Arg.Any<BankConnection>(), Arg.Any<BankNotification>(), Arg.Any<int?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncAsync_ConnectionWithoutCapital_KeepsStatusNone()
    {
        // Arrange
        ReturnTransactions(Transaction("tx-1", Now.AddMinutes(-2)));

        // Act
        await _service.SyncAsync(CancellationToken.None);

        // Assert
        _notificationsMock.Received(1).CreateRange(Arg.Is<IEnumerable<BankNotification>>(items =>
            items.Single().ImportStatus == BankImportStatus.None));
    }

    [Fact]
    public async Task SyncAsync_AutoImportEnabled_ImportsEveryNewTransaction()
    {
        // Arrange
        _connection.CapitalId = 3;
        _connection.AutoImport = true;
        _importerMock
            .ImportAsync(Arg.Any<BankConnection>(), Arg.Any<BankNotification>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        ReturnTransactions(Transaction("tx-1", Now.AddMinutes(-2)), Transaction("tx-2", Now.AddMinutes(-1)));

        // Act
        await _service.SyncAsync(CancellationToken.None);

        // Assert
        await _importerMock.Received(2).ImportAsync(
            _connection, Arg.Any<BankNotification>(), null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncAsync_MonobankConnection_UsesProviderAccountId()
    {
        // Arrange
        var monobank = Substitute.For<IBankStatementProvider>();
        monobank.Provider.Returns(BankProvider.Monobank);
        monobank
            .GetTransactionsAsync("token", "acc-id", Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<BankTransaction>>([Transaction("mono:1", Now.AddMinutes(-2))]));
        _connection.Provider = BankProvider.Monobank;
        _connection.ProviderAccountId = "acc-id";
        var service = new BankTransactionSyncService(
            _connectionsMock, _notificationsMock, _settingsMock, [_bankMock, monobank], _importerMock,
            _protectorMock, _emailMock, _publisherMock, _clockMock, _unitOfWorkMock);

        // Act
        var created = await service.SyncAsync(CancellationToken.None);

        // Assert
        created.Should().Be(1);
        await _bankMock.DidNotReceive().GetTransactionsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }

    private static BankTransaction Transaction(string id, DateTimeOffset occurredAt)
        => new(id, BankTransactionDirection.Outgoing, 150m, "UAH", "John", "Rent", occurredAt);

    private void ReturnTransactions(params BankTransaction[] transactions)
        => _bankMock
            .GetTransactionsAsync("token", _connection.AccountIban, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<BankTransaction>>(transactions));
}
