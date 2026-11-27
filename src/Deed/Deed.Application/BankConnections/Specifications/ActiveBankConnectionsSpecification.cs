using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Application.BankConnections.Specifications;

internal sealed class ActiveBankConnectionsSpecification : BaseSpecification<BankConnection>
{
    public ActiveBankConnectionsSpecification(string? createdBy = null, bool includeExcluded = false, BankProvider? provider = null)
        : base(c => c.Status == BankConnectionStatus.Active && (includeExcluded || !c.IsExcluded) && (provider == null ? c.Provider != BankProvider.PrivatMail : c.Provider == provider) && (createdBy == null || c.CreatedBy == createdBy))
    {
        Tracking = true;
        ApplyOrderBy(c => c.LastSyncAt);
    }
}
