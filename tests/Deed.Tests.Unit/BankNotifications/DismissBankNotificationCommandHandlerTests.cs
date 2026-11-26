using Deed.Application.Auth;
using Deed.Application.BankNotifications.Commands.Dismiss;
using Deed.Application.BankNotifications.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankNotifications;

public sealed class DismissBankNotificationCommandHandlerTests
{
    private readonly IBankNotificationRepository _repositoryMock = Substitute.For<IBankNotificationRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly DismissBankNotificationCommandHandler _handler;

    public DismissBankNotificationCommandHandlerTests()
    {
        var user = Substitute.For<IUser>();
        user.Name.Returns("owner");

        _handler = new DismissBankNotificationCommandHandler(user, _repositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_PendingNotification_IsDismissedAndMarkedRead()
    {
        // Arrange
        var notification = new BankNotification(1) { ExternalId = "x", ImportStatus = BankImportStatus.Pending };
        _repositoryMock.GetAsync(Arg.Any<BankNotificationsByUserSpecification>(), Arg.Any<CancellationToken>()).Returns(notification);

        // Act
        var result = await _handler.Handle(new DismissBankNotificationCommand(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.ImportStatus.Should().Be(BankImportStatus.Dismissed);
        notification.IsRead.Should().BeTrue();
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownNotification_ReturnsNotFound()
    {
        // Arrange
        _repositoryMock.GetAsync(Arg.Any<BankNotificationsByUserSpecification>(), Arg.Any<CancellationToken>()).Returns((BankNotification?)null);

        // Act
        var result = await _handler.Handle(new DismissBankNotificationCommand(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(BankImportStatus.None)]
    [InlineData(BankImportStatus.Imported)]
    [InlineData(BankImportStatus.Dismissed)]
    public async Task Handle_NotPending_ReturnsConflict(BankImportStatus status)
    {
        // Arrange
        var notification = new BankNotification(1) { ExternalId = "x", ImportStatus = status };
        _repositoryMock.GetAsync(Arg.Any<BankNotificationsByUserSpecification>(), Arg.Any<CancellationToken>()).Returns(notification);

        // Act
        var result = await _handler.Handle(new DismissBankNotificationCommand(1), CancellationToken.None);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Bank.NotPending);
    }
}
