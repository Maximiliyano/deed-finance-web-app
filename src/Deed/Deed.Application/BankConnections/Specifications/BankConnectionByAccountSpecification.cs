using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Application.BankConnections.Specifications;

internal sealed class BankConnectionByAccountSpecification : BaseSpecification<BankConnection>
{
    public BankConnectionByAccountSpecification(string createdBy, BankProvider provider, string account, bool tracking = false)
        : base(c => c.CreatedBy == createdBy && c.Provider == provider && c.AccountIban == account)
    {
        Tracking = tracking;
    }
}
