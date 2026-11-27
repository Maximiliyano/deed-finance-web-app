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

        // Act
        var kept = new[] { Notification(1), Notification(2), Notification(3), Notification(null) }.Select(matches).ToArray();

        // Assert
        kept.Should().Equal(true, true, false, false);
    }

    [Fact]
    public void ListSpecification_UnassignedAllowed_KeepsRowsWithoutAConnection()
    {
        // Arrange
        var matches = new BankTransactionsByUserSpecification("owner", connectionIds: [1], includeUnassigned: true).Criteria!.Compile();

        // Act
        var kept = new[] { Notification(null), Notification(1), Notification(2) }.Select(matches).ToArray();

        // Assert
        kept.Should().Equal(true, true, false);
    }

    [Fact]
    public void ListSpecification_NoScope_KeepsEverything()
    {
        // Arrange
        var matches = new BankTransactionsByUserSpecification("owner").Criteria!.Compile();

        // Act
        var kept = new[] { Notification(3), Notification(null) }.Select(matches).ToArray();

        // Assert
        kept.Should().Equal(true, true);
    }

    [Fact]
    public void ListSpecification_NoConnectionsLeft_MatchesNothingAssigned()
    {
        // Arrange
        var matches = new BankTransactionsByUserSpecification("owner", connectionIds: [], includeUnassigned: false).Criteria!.Compile();

        // Act
        var kept = matches(Notification(1));

        // Assert
        kept.Should().BeFalse();
    }

    [Fact]
    public void ListSpecification_AccountInsideScope_AppliesBothFilters()
    {
        // Arrange
        var matches = new BankTransactionsByUserSpecification("owner", connectionId: 2, connectionIds: [1, 2], includeUnassigned: false).Criteria!.Compile();

        // Act
        var kept = new[] { Notification(2), Notification(1) }.Select(matches).ToArray();

        // Assert
        kept.Should().Equal(true, false);
    }

    [Fact]
    public void ListSpecification_Currency_KeepsOnlyThatCurrency()
    {
        // Arrange
        var matches = new BankTransactionsByUserSpecification("owner", currency: "USD").Criteria!.Compile();

        // Act
        var kept = new[] { Notification(1, "USD"), Notification(1, "UAH"), Notification(1, null) }.Select(matches).ToArray();

        // Assert
        kept.Should().Equal(true, false, false);
    }

    [Fact]
    public void ListSpecification_Period_KeepsRowsFromStartUpToButExcludingEnd()
    {
        // Arrange
        var from = new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.Zero);
        var matches = new BankTransactionsByUserSpecification("owner", from: from, to: to).Criteria!.Compile();

        // Act
        var kept = new[] { At(from.AddTicks(-1)), At(from), At(to.AddTicks(-1)), At(to) }.Select(matches).ToArray();

        // Assert
        kept.Should().Equal(false, true, true, false);
    }

    [Fact]
    public void ListSpecification_OpenEndedPeriod_OnlyAppliesTheGivenBound()
    {
        // Arrange
        var from = new DateTimeOffset(2026, 11, 10, 0, 0, 0, TimeSpan.Zero);
        var matches = new BankTransactionsByUserSpecification("owner", from: from).Criteria!.Compile();

        // Act
        var kept = new[] { At(from.AddDays(-1)), At(from.AddYears(5)) }.Select(matches).ToArray();

        // Assert
        kept.Should().Equal(false, true);
    }

    [Theory]
    [InlineData("SILPO", true)]
    [InlineData("CARD PAY", true)]
    [InlineData("KYIV", true)]
    [InlineData("BOLT", false)]
    public void ListSpecification_Search_MatchesCounterpartyPurposeAndAccount(string search, bool expected)
    {
        // Arrange
        var matches = new BankTransactionsByUserSpecification("owner", search: search).Criteria!.Compile();
        var item = Described("Silpo Market", "Card payment", "Kyiv wallet");

        // Act
        var kept = matches(item);

        // Assert
        kept.Should().Be(expected);
    }

    [Fact]
    public void ListSpecification_SearchWithAmount_MatchesTheAmountEvenWhenTextDoesNot()
    {
        // Arrange
        var matches = new BankTransactionsByUserSpecification("owner", search: "312.40", amount: 312.40m).Criteria!.Compile();
        var hit = Described("Silpo", null, null);
        hit.Amount = 312.40m;
        var miss = Described("Silpo", null, null);
        miss.Amount = 99m;

        // Act
        var kept = new[] { hit, miss }.Select(matches).ToArray();

        // Assert
        kept.Should().Equal(true, false);
    }

    [Fact]
    public void ListSpecification_SearchWithoutAmount_NeverMatchesByAmount()
    {
        // Arrange
        var matches = new BankTransactionsByUserSpecification("owner", search: "ZZZ").Criteria!.Compile();
        var item = Described("Silpo", null, null);

        // Act
        var kept = matches(item);

        // Assert
        kept.Should().BeFalse();
    }

    [Fact]
    public void ListSpecification_SearchWithMissingTextFields_DoesNotThrow()
    {
        // Arrange
        var matches = new BankTransactionsByUserSpecification("owner", search: "SILPO").Criteria!.Compile();
        var item = Described(null, null, null);

        // Act
        var kept = matches(item);

        // Assert
        kept.Should().BeFalse();
    }

    [Fact]
    public void SummarySpecification_ConnectionIds_KeepsOnlyThoseConnections()
    {
        // Arrange
        var matches = new BankTransactionsSummarySpecification("owner", null, Now.AddDays(-30), Now, [5], false).Criteria!.Compile();

        // Act
        var kept = new[] { Notification(5), Notification(6), Notification(null) }.Select(matches).ToArray();

        // Assert
        kept.Should().Equal(true, false, false);
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

    private static BankNotification At(DateTimeOffset occurredAt)
    {
        var item = Notification(1);
        item.OccurredAt = occurredAt;

        return item;
    }

    private static BankNotification Described(string? counterparty, string? purpose, string? accountLabel)
    {
        var item = Notification(1);
        item.Counterparty = counterparty;
        item.Purpose = purpose;
        item.AccountLabel = accountLabel;

        return item;
    }

    private static BankNotification Notification(int? connectionId, string? currency = "UAH")
        => new()
        {
            ExternalId = "x",
            Currency = currency,
            Kind = BankNotificationKind.Transaction,
            Direction = BankTransactionDirection.Outgoing,
            OccurredAt = Now.AddDays(-1),
            BankConnectionId = connectionId,
            CreatedBy = "owner"
        };
}
