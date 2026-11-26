using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Application.BankConnections.Specifications;

internal sealed class ActiveBankConnectionsSpecification : BaseSpecification<BankConnection>
{
    public ActiveBankConnectionsSpecification(string? createdBy = null)
        : base(c => c.Status == BankConnectionStatus.Active && (createdBy == null || c.CreatedBy == createdBy))
    {
        Tracking = true;
    }
}
