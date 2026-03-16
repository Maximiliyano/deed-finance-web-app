using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Goals.Specifications;

internal sealed class GoalsByUserSpecification : BaseSpecification<Goal>
{
    public GoalsByUserSpecification(string createdBy)
        : base(g => g.CreatedBy == createdBy)
    {
        ApplyOrderBy(g => g.OrderIndex);
    }
}
