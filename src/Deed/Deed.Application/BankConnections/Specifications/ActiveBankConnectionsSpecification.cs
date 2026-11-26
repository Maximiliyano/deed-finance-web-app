using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Application.BankConnections.Specifications;

internal sealed class ActiveBankConnectionsSpecification : BaseSpecification<BankConnection>
{
    public ActiveBankConnectionsSpecification()
        : base(c => c.Status == BankConnectionStatus.Active)
    {
        Tracking = true;
    }
}
