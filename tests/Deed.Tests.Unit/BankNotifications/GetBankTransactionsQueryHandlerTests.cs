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

    [Fact]
    public async Task Handle_Search_IsTrimmedUppercasedAndMatchesText()
    {
        // Arrange
        var query = new GetBankTransactionsQuery(Search: "  silpo ");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _specifications.Should().HaveCount(2);
        foreach (var specification in _specifications)
        {
            var matches = specification.Criteria!.Compile();

            matches(Notification(1, counterparty: "Silpo Market")).Should().BeTrue();
            matches(Notification(1, counterparty: "Bolt")).Should().BeFalse();
        }
    }

    [Theory]
    [InlineData("312.40")]
    [InlineData("312,40")]
    [InlineData("-312.40")]
    public async Task Handle_NumericSearch_AlsoMatchesTheAmount(string search)
    {
        // Arrange
        var query = new GetBankTransactionsQuery(Search: search);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        foreach (var specification in _specifications)
        {
            var matches = specification.Criteria!.Compile();

            matches(Notification(1, amount: 312.40m)).Should().BeTrue();
            matches(Notification(1, amount: 100m)).Should().BeFalse();
        }
    }

    [Fact]
    public async Task Handle_BlankSearch_IsIgnored()
    {
        // Arrange
        var query = new GetBankTransactionsQuery(Search: "   ");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        foreach (var specification in _specifications)
        {
            specification.Criteria!.Compile()(Notification(1, counterparty: "Anything")).Should().BeTrue();
        }
    }

    [Fact]
    public async Task Handle_Period_KeepsTheStartAndExcludesTheEnd()
    {
        // Arrange
        var from = new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.Zero);
        var query = new GetBankTransactionsQuery(From: from, To: to);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        foreach (var specification in _specifications)
        {
            var matches = specification.Criteria!.Compile();

            matches(Notification(1, occurredAt: from)).Should().BeTrue();
            matches(Notification(1, occurredAt: to.AddTicks(-1))).Should().BeTrue();
            matches(Notification(1, occurredAt: to)).Should().BeFalse();
            matches(Notification(1, occurredAt: from.AddTicks(-1))).Should().BeFalse();
        }
    }

    [Fact]
    public async Task Handle_SearchAndPeriod_RunsOneCountAndOnePageQuery()
    {
        // Arrange
        var query = new GetBankTransactionsQuery(Search: "silpo", From: Now.AddDays(-10), To: Now);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _repositoryMock.Received(1).CountAsync(Arg.Any<ISpecification<BankNotification>>(), Arg.Any<CancellationToken>());
        await _repositoryMock.Received(1).GetAllAsync(Arg.Any<ISpecification<BankNotification>>(), Arg.Any<CancellationToken>());
    }

    private static BankNotification Notification(int? connectionId, string currency = "UAH", string? counterparty = null, decimal amount = 0, DateTimeOffset? occurredAt = null)
        => new()
        {
            ExternalId = "x",
            Kind = BankNotificationKind.Transaction,
            Currency = currency,
            Counterparty = counterparty,
            Amount = amount,
            OccurredAt = occurredAt ?? Now.AddDays(-1),
            BankConnectionId = connectionId,
            CreatedBy = "owner"
        };
}
