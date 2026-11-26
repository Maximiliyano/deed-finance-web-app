using Deed.Application.Auth;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.BankNotifications.Queries.GetAll;
using Deed.Application.BankNotifications.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankNotifications;

public sealed class GetBankNotificationsQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly IUser _userMock = Substitute.For<IUser>();
    private readonly IBankNotificationRepository _repositoryMock = Substitute.For<IBankNotificationRepository>();
    private readonly IBankConnectionRepository _connectionsMock = Substitute.For<IBankConnectionRepository>();

    private readonly GetBankNotificationsQueryHandler _handler;

    public GetBankNotificationsQueryHandlerTests()
    {
        _userMock.Name.Returns("owner");
        _connectionsMock
            .GetAllAsync(Arg.Any<BankConnectionByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                new BankConnection(1) { EncryptedToken = "x", AccountIban = "UA1" },
                new BankConnection(2) { EncryptedToken = "x", AccountIban = "UA2", IsExcluded = true }
            ]);
        _repositoryMock
            .GetAllAsync(Arg.Any<BankNotificationsByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<BankNotification>());

        _handler = new GetBankNotificationsQueryHandler(_userMock, _repositoryMock, _connectionsMock);
    }

    [Fact]
    public async Task Handle_ExcludedConnection_IsHiddenFromTheListAndTheUnreadCount()
    {
        // Arrange
        var captured = new List<ISpecification<BankNotification>>();
        _repositoryMock
            .GetAllAsync(Arg.Do<ISpecification<BankNotification>>(captured.Add), Arg.Any<CancellationToken>())
            .Returns(new List<BankNotification>());
        _repositoryMock
            .CountAsync(Arg.Do<ISpecification<BankNotification>>(captured.Add), Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        var result = await _handler.Handle(new GetBankNotificationsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        captured.Should().HaveCount(2);
        foreach (var specification in captured)
        {
            var matches = specification.Criteria!.Compile();
            matches(Notification(1)).Should().BeTrue();
            matches(Notification(2)).Should().BeFalse();
            matches(Notification(null)).Should().BeTrue();
        }
    }

    private static BankNotification Notification(int? connectionId)
        => new()
        {
            ExternalId = "x",
            Kind = BankNotificationKind.Transaction,
            OccurredAt = Now.AddDays(-1),
            BankConnectionId = connectionId,
            CreatedBy = "owner"
        };
}
