using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Debts.Specifications;

internal sealed class DebtByIdSpecification : BaseSpecification<Debt>
{
    public DebtByIdSpecification(int id, string? createdBy = null)
        : base(d => d.Id == id && (createdBy == null || d.CreatedBy == createdBy))
    {
    }
}
