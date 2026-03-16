using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Debts.Specifications;

internal sealed class DebtsByUserSpecification : BaseSpecification<Debt>
{
    public DebtsByUserSpecification(string createdBy)
        : base(d => d.CreatedBy == createdBy)
    {
        AddInclude(d => d.Capital!);
        ApplyOrderBy(d => d.OrderIndex);
    }
}
