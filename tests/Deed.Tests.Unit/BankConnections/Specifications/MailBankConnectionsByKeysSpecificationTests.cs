using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using FluentAssertions;

namespace Deed.Tests.Unit.BankConnections.Specifications;

public sealed class MailBankConnectionsByKeysSpecificationTests
{
    private static BankConnection Connection(int id, string? key, BankProvider provider = BankProvider.PrivatMail, bool excluded = false, BankConnectionStatus status = BankConnectionStatus.Active)
        => new(id) { EncryptedToken = string.Empty, AccountIban = $"UA{id}", ProviderAccountId = key, Provider = provider, IsExcluded = excluded, Status = status };

    [Fact]
    public void Criteria_MatchesOnlyActiveCountedMailConnectionsWithAKnownKey()
    {
        // Arrange
        var matches = new MailBankConnectionsByKeysSpecification(["KEY1"]).Criteria!.Compile();

        // Act
        var kept = new[]
        {
            matches(Connection(1, "KEY1")),
            matches(Connection(2, "KEY2")),
            matches(Connection(3, null)),
            matches(Connection(4, "KEY1", BankProvider.Monobank)),
            matches(Connection(5, "KEY1", excluded: true)),
            matches(Connection(6, "KEY1", status: BankConnectionStatus.TokenInvalid))
        };

        // Assert
        kept.Should().Equal(true, false, false, false, false, false);
    }

    [Fact]
    public void Specification_LoadsTrackedEntitiesForUpdates()
    {
        // Arrange
        // Act
        var specification = new MailBankConnectionsByKeysSpecification(["KEY1"]);

        // Assert
        specification.Tracking.Should().BeTrue();
    }
}
