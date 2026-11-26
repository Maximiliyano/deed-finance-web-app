using Deed.Application.Auth;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.BankNotifications.Queries.GetPage;
using Deed.Application.BankNotifications.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Deed.Tests.Unit.BankNotifications;

public sealed class GetBankTransactionsQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly IBankNotificationRepository _repositoryMock = Substitute.For<IBankNotificationRepository>();
    private readonly IBankConnectionRepository _connectionsMock = Substitute.For<IBankConnectionRepository>();
    private readonly List<ISpecification<BankNotification>> _specifications = [];

    private readonly GetBankTransactionsQueryHandler _handler;

    public GetBankTransactionsQueryHandlerTests()
    {
        var user = Substitute.For<IUser>();
        user.Name.Returns("owner");
        _connectionsMock
            .GetAllAsync(Arg.Any<BankConnectionByUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                new BankConnection(1) { EncryptedToken = "x", AccountIban = "UA1", Provider = BankProvider.PrivatAutoclient },
                new BankConnection(2) { EncryptedToken = "x", AccountIban = "UA2", Provider = BankProvider.Monobank },
                new BankConnection(3) { EncryptedToken = "x", AccountIban = "UA3", Provider = BankProvider.PrivatAutoclient, IsExcluded = true }
            ]);
        _repositoryMock
            .CountAsync(Arg.Do<ISpecification<BankNotification>>(_specifications.Add), Arg.Any<CancellationToken>())
            .Returns(42);
        _repositoryMock
            .GetAllAsync(Arg.Do<ISpecification<BankNotification>>(_specifications.Add), Arg.Any<CancellationToken>())
            .Returns(new List<BankNotification>());

        _handler = new GetBankTransactionsQueryHandler(user, _repositoryMock, _connectionsMock);
    }

    [Fact]
    public async Task Handle_PageSizeAboveTheLimit_IsClamped()
    {
        // Arrange
        var query = new GetBankTransactionsQuery(Page: 0, PageSize: 5000);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(100);
        result.Value.Total.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ProviderTab_ShowsOnlyThatProvidersCountedConnections()
    {
        // Arrange
        var query = new GetBankTransactionsQuery(Provider: "PrivatAutoclient");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _specifications.Should().NotBeEmpty();
        foreach (var specification in _specifications)
        {
            var matches = specification.Criteria!.Compile();

            matches(Notification(1)).Should().BeTrue();
            matches(Notification(2)).Should().BeFalse();
            matches(Notification(3)).Should().BeFalse();
            matches(Notification(null)).Should().BeFalse();
        }
    }

    [Fact]
    public async Task Handle_AllBanks_HidesExcludedConnectionsButKeepsUnassignedRows()
    {
        // Arrange
        var query = new GetBankTransactionsQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        foreach (var specification in _specifications)
        {
            var matches = specification.Criteria!.Compile();

            matches(Notification(1)).Should().BeTrue();
            matches(Notification(3)).Should().BeFalse();
            matches(Notification(null)).Should().BeTrue();
        }
    }

    [Fact]
    public async Task Handle_Currency_IsNormalizedAndFiltered()
    {
        // Arrange
        var query = new GetBankTransactionsQuery(Currency: " usd ");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        foreach (var specification in _specifications)
        {
            var matches = specification.Criteria!.Compile();

            matches(Notification(1, "USD")).Should().BeTrue();
            matches(Notification(1, "UAH")).Should().BeFalse();
        }
    }

    private static BankNotification Notification(int? connectionId, string currency = "UAH")
        => new()
        {
            ExternalId = "x",
            Kind = BankNotificationKind.Transaction,
            Currency = currency,
            OccurredAt = Now.AddDays(-1),
            BankConnectionId = connectionId,
            CreatedBy = "owner"
        };
}
