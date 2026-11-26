using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.BankConnections.Specifications;

internal sealed class BankAccountsByUserSpecification : BaseSpecification<BankAccount>
{
    public BankAccountsByUserSpecification(string createdBy, bool tracking = false)
        : base(a => a.CreatedBy == createdBy)
    {
        Tracking = tracking;
        ApplyOrderBy(a => a.Id);
    }
}
