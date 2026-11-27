using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using FluentAssertions;

namespace Deed.Tests.Unit.BankConnections.Specifications;

public sealed class ActiveBankConnectionsSpecificationTests
{
    private readonly BankConnection _excluded = new(1) { EncryptedToken = "x", AccountIban = "UA1", Status = BankConnectionStatus.Active, IsExcluded = true, CreatedBy = "owner" };
    private readonly BankConnection _counted = new(2) { EncryptedToken = "x", AccountIban = "UA2", Status = BankConnectionStatus.Active, CreatedBy = "owner" };
    private readonly BankConnection _monobank = new(3) { EncryptedToken = "x", AccountIban = "UA3", Status = BankConnectionStatus.Active, Provider = BankProvider.Monobank, CreatedBy = "owner" };
    private readonly BankConnection _invalid = new(4) { EncryptedToken = "x", AccountIban = "UA4", Status = BankConnectionStatus.TokenInvalid, CreatedBy = "owner" };

    [Fact]
    public void Criteria_ExcludedConnection_IsSkippedUnlessRequested()
    {
        // Arrange
        var normal = new ActiveBankConnectionsSpecification("owner").Criteria!.Compile();
        var withExcluded = new ActiveBankConnectionsSpecification("owner", includeExcluded: true).Criteria!.Compile();

        // Act
        var kept = new[] { normal(_excluded), normal(_counted), withExcluded(_excluded) };

        // Assert
        kept.Should().Equal(false, true, true);
    }

    [Fact]
    public void Criteria_Provider_KeepsOnlyThatProvider()
    {
        // Arrange
        var matches = new ActiveBankConnectionsSpecification(provider: BankProvider.Monobank).Criteria!.Compile();

        // Act
        var kept = new[] { matches(_monobank), matches(_counted) };

        // Assert
        kept.Should().Equal(true, false);
    }

    [Fact]
    public void Criteria_NoProvider_SkipsTheTokenlessMailConnections()
    {
        // Arrange
        var mail = new BankConnection(5) { EncryptedToken = string.Empty, AccountIban = "UA5", Status = BankConnectionStatus.Active, Provider = BankProvider.PrivatMail, CreatedBy = "owner" };
        var matches = new ActiveBankConnectionsSpecification("owner").Criteria!.Compile();
        var matchesMail = new ActiveBankConnectionsSpecification(provider: BankProvider.PrivatMail).Criteria!.Compile();

        // Act
        var kept = new[] { matches(mail), matches(_counted), matchesMail(mail) };

        // Assert
        kept.Should().Equal(false, true, true);
    }

    [Fact]
    public void Criteria_TokenInvalidOrOtherUser_IsSkipped()
    {
        // Arrange
        var matches = new ActiveBankConnectionsSpecification("someone-else").Criteria!.Compile();

        // Act
        var kept = new[] { matches(_invalid), matches(_counted) };

        // Assert
        kept.Should().Equal(false, false);
    }
}
