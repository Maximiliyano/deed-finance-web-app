using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Debts.Specifications;

internal sealed class UnpaidDebtsByUserSpecification : BaseSpecification<Debt>
{
    public UnpaidDebtsByUserSpecification(string createdBy)
        : base(d => d.CreatedBy == createdBy && !d.IsPaid)
    {
        ApplyOrderBy(d => d.OrderIndex);
    }
}
