using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Application.BankConnections.Specifications;

internal sealed class BackfillBankConnectionsSpecification : BaseSpecification<BankConnection>
{
    public BackfillBankConnectionsSpecification()
        : base(c => c.Status == BankConnectionStatus.Active && !c.IsExcluded && c.BackfillCursor != null)
    {
        Tracking = true;
    }
}
