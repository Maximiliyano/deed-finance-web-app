using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Goals.Specifications;

internal sealed class GoalByIdSpecification : BaseSpecification<Goal>
{
    public GoalByIdSpecification(int id, string? createdBy = null)
        : base(g => g.Id == id && (createdBy == null || g.CreatedBy == createdBy))
    {
    }
}
