using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.BudgetEstimations.Specifications;

internal sealed class BudgetEstimationsByUserSpecification : BaseSpecification<BudgetEstimation>
{
    public BudgetEstimationsByUserSpecification(string createdBy, bool includeCapital = false)
        : base(e => e.CreatedBy == createdBy)
    {
        if (includeCapital)
        {
            AddInclude(e => e.Capital!);
        }

        ApplyOrderBy(e => e.OrderIndex);
    }
}
