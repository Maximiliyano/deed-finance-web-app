using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.BudgetEstimations.Specifications;

internal sealed class BudgetEstimationsByUserSpecification : BaseSpecification<BudgetEstimation>
{
    public BudgetEstimationsByUserSpecification(
        DateTime periodStart,
        DateTime periodEnd,
        string createdBy,
        bool includeCapital = false)
        : base(e => 
            e.CreatedAt.Date >= periodStart.Date && e.CreatedAt.Date <= periodEnd.Date &&
            e.CreatedBy == createdBy)
    {
        if (includeCapital)
        {
            AddInclude(e => e.Capital!);
        }

        ApplyOrderBy(e => e.OrderIndex);
    }
}
