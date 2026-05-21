using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.UtilityBills.Specifications;

internal sealed class UtilityBillByIdSpecification : BaseSpecification<UtilityBill>
{
    public UtilityBillByIdSpecification(int id, string? createdBy = null)
        : base(b => b.Id == id && (createdBy == null || b.CreatedBy == createdBy))
    {
    }
}
