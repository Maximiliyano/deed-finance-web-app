using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.BudgetEstimations.Specifications;

internal sealed class BudgetEstimationByIdSpecification : BaseSpecification<BudgetEstimation>
{
    public BudgetEstimationByIdSpecification(int id, string? createdBy = null)
        : base(e => e.Id == id && (createdBy == null || e.CreatedBy == createdBy))
    {
    }
}
