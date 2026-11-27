using Deed.Application.Auth;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.BankNotifications.Commands.Import;
using Deed.Application.BankNotifications.Import;
using Deed.Application.BankNotifications.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankNotifications;

public sealed class ImportBankNotificationCommandHandlerTests
{
    private readonly IBankNotificationRepository _notificationsMock = Substitute.For<IBankNotificationRepository>();
    private readonly IBankConnectionRepository _connectionsMock = Substitute.For<IBankConnectionRepository>();
    private readonly ICategoryRepository _categoriesMock = Substitute.For<ICategoryRepository>();
    private readonly IBankTransactionImporter _importerMock = Substitute.For<IBankTransactionImporter>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private readonly ImportBankNotificationCommandHandler _handler;
    private readonly BankNotification _notification = new(1)
    {
        ExternalId = "x",
        ImportStatus = BankImportStatus.Pending,
        Direction = BankTransactionDirection.Outgoing,
        BankConnectionId = 7
    };

    public ImportBankNotificationCommandHandlerTests()
    {
        var user = Substitute.For<IUser>();
        user.Id.Returns("owner");
        _notificationsMock.GetAsync(Arg.Any<BankNotificationsByUserSpecification>(), Arg.Any<CancellationToken>()).Returns(_notification);
        _connectionsMock
            .GetAsync(Arg.Any<BankConnectionByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new BankConnection(7) { EncryptedToken = "x", AccountIban = "UA1", CapitalId = 3 });
        _importerMock
            .ImportAsync(Arg.Any<BankConnection>(), Arg.Any<BankNotification>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _handler = new ImportBankNotificationCommandHandler(user, _notificationsMock, _connectionsMock, _categoriesMock, _importerMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_PendingNotification_IsImportedAndMarkedRead()
    {
        // Arrange
        var command = new ImportBankNotificationCommand(1, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _notification.IsRead.Should().BeTrue();
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConnectionNotCounted_ReturnsNotConnected()
    {
        // Arrange
        _connectionsMock
            .GetAsync(Arg.Any<BankConnectionByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new BankConnection(7) { EncryptedToken = "x", AccountIban = "UA1", CapitalId = 3, IsExcluded = true });

        // Act
        var result = await _handler.Handle(new ImportBankNotificationCommand(1, null), CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.NotConnected);
        await _importerMock.DidNotReceive().ImportAsync(Arg.Any<BankConnection>(), Arg.Any<BankNotification>(), Arg.Any<int?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotificationWithoutConnection_ReturnsCapitalNotLinked()
    {
        // Arrange
        _notification.BankConnectionId = null;

        // Act
        var result = await _handler.Handle(new ImportBankNotificationCommand(1, null), CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.CapitalNotLinked);
    }

    [Fact]
    public async Task Handle_AlreadyImported_ReturnsConflict()
    {
        // Arrange
        _notification.ImportStatus = BankImportStatus.Imported;

        // Act
        var result = await _handler.Handle(new ImportBankNotificationCommand(1, null), CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.NotPending);
    }

    [Fact]
    public async Task Handle_ImporterFails_ReturnsItsErrorWithoutSaving()
    {
        // Arrange
        _importerMock
            .ImportAsync(Arg.Any<BankConnection>(), Arg.Any<BankNotification>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(DomainErrors.Bank.CurrencyMismatch));

        // Act
        var result = await _handler.Handle(new ImportBankNotificationCommand(1, null), CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.CurrencyMismatch);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
