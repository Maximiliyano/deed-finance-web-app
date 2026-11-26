using Deed.Application.BankNotifications;
using Deed.Application.BankNotifications.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using FluentAssertions;

namespace Deed.Tests.Unit.BankNotifications;

public sealed class BankTransactionSpecificationTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ListSpecification_ConnectionIds_KeepsOnlyThoseAndHidesOthers()
    {
        // Arrange
        var matches = new BankTransactionsByUserSpecification("owner", connectionIds: [1, 2], includeUnassigned: false).Criteria!.Compile();

        // Act & Assert
        matches(Notification(1)).Should().BeTrue();
        matches(Notification(2)).Should().BeTrue();
        matches(Notification(3)).Should().BeFalse();
        matches(Notification(null)).Should().BeFalse();
    }

    [Fact]
    public void ListSpecification_UnassignedAllowed_KeepsRowsWithoutAConnection()
    {
        // Arrange
        var matches = new BankTransactionsByUserSpecification("owner", connectionIds: [1], includeUnassigned: true).Criteria!.Compile();

        // Act & Assert
        matches(Notification(null)).Should().BeTrue();
        matches(Notification(1)).Should().BeTrue();
        matches(Notification(2)).Should().BeFalse();
    }

    [Fact]
    public void ListSpecification_NoScope_KeepsEverything()
    {
        // Arrange
        var matches = new BankTransactionsByUserSpecification("owner").Criteria!.Compile();

        // Act & Assert
        matches(Notification(3)).Should().BeTrue();
        matches(Notification(null)).Should().BeTrue();
    }

    [Fact]
    public void ListSpecification_NoConnectionsLeft_MatchesNothingAssigned()
    {
        // Arrange
        var matches = new BankTransactionsByUserSpecification("owner", connectionIds: [], includeUnassigned: false).Criteria!.Compile();

        // Act & Assert
        matches(Notification(1)).Should().BeFalse();
    }

    [Fact]
    public void ListSpecification_AccountInsideScope_AppliesBothFilters()
    {
        // Arrange
        var matches = new BankTransactionsByUserSpecification("owner", connectionId: 2, connectionIds: [1, 2], includeUnassigned: false).Criteria!.Compile();

        // Act & Assert
        matches(Notification(2)).Should().BeTrue();
        matches(Notification(1)).Should().BeFalse();
    }

    [Fact]
    public void SummarySpecification_ConnectionIds_KeepsOnlyThoseConnections()
    {
        // Arrange
        var matches = new BankTransactionsSummarySpecification("owner", null, Now.AddDays(-30), Now, [5], false).Criteria!.Compile();

        // Act & Assert
        matches(Notification(5)).Should().BeTrue();
        matches(Notification(6)).Should().BeFalse();
        matches(Notification(null)).Should().BeFalse();
    }

    [Fact]
    public void Scope_ExcludedAndOtherProviderConnections_AreLeftOut()
    {
        // Arrange
        BankConnection[] connections =
        [
            new(1) { EncryptedToken = "x", AccountIban = "UA1", Provider = BankProvider.PrivatAutoclient },
            new(2) { EncryptedToken = "x", AccountIban = "UA2", Provider = BankProvider.PrivatAutoclient, IsExcluded = true },
            new(3) { EncryptedToken = "x", AccountIban = "UA3", Provider = BankProvider.Monobank }
        ];

        // Act
        var all = BankConnectionScope.Resolve(connections, null, null);
        var privat = BankConnectionScope.Resolve(connections, "privatautoclient", null);
        var account = BankConnectionScope.Resolve(connections, null, 3);

        // Assert
        all.Ids.Should().BeEquivalentTo([1, 3]);
        all.IncludeUnassigned.Should().BeTrue();
        privat.Ids.Should().BeEquivalentTo([1]);
        privat.IncludeUnassigned.Should().BeFalse();
        account.IncludeUnassigned.Should().BeFalse();
    }

    private static BankNotification Notification(int? connectionId)
        => new()
        {
            ExternalId = "x",
            Kind = BankNotificationKind.Transaction,
            Direction = BankTransactionDirection.Outgoing,
            OccurredAt = Now.AddDays(-1),
            BankConnectionId = connectionId,
            CreatedBy = "owner"
        };
}
