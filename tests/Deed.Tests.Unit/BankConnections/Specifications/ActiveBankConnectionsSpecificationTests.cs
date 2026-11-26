using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using FluentAssertions;

namespace Deed.Tests.Unit.BankConnections.Specifications;

public sealed class ActiveBankConnectionsSpecificationTests
{
    [Fact]
    public void Criteria_ExcludedConnection_IsSkippedUnlessRequested()
    {
        // Arrange
        var excluded = new BankConnection(1) { EncryptedToken = "x", AccountIban = "UA1", Status = BankConnectionStatus.Active, IsExcluded = true, CreatedBy = "owner" };
        var counted = new BankConnection(2) { EncryptedToken = "x", AccountIban = "UA2", Status = BankConnectionStatus.Active, CreatedBy = "owner" };

        var normal = new ActiveBankConnectionsSpecification("owner").Criteria!.Compile();
        var withExcluded = new ActiveBankConnectionsSpecification("owner", includeExcluded: true).Criteria!.Compile();

        // Act & Assert
        normal(excluded).Should().BeFalse();
        normal(counted).Should().BeTrue();
        withExcluded(excluded).Should().BeTrue();
    }
}
