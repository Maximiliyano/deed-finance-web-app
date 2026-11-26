using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.BankConnections.Specifications;

internal sealed class BankConnectionByIdSpecification : BaseSpecification<BankConnection>
{
    public BankConnectionByIdSpecification(int id, string createdBy, bool tracking = false)
        : base(c => c.Id == id && c.CreatedBy == createdBy)
    {
        Tracking = tracking;
    }
}
