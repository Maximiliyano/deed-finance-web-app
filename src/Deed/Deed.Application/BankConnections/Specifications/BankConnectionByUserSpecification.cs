using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.BankConnections.Specifications;

internal sealed class BankConnectionByUserSpecification : BaseSpecification<BankConnection>
{
    public BankConnectionByUserSpecification(string createdBy, bool tracking = false)
        : base(c => c.CreatedBy == createdBy)
    {
        Tracking = tracking;
    }
}
