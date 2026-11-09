using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Deed.Application.Incomes.Specifications;

internal sealed class IncomeByIdSpecification : BaseSpecification<Income>
{
    public IncomeByIdSpecification(int id, string? createdBy = null, bool includeTags = false, bool enableTracking = false)
        : base(BuildCriteria(id, createdBy, includeTags))
    {
        AddInclude(i => i.Capital!);
        AddInclude(i => i.Category!);

        if (includeTags)
        {
            Includes.Add(q => q.Include(i => i.Tags).ThenInclude(t => t.Tag));
            IgnoreQueryFilter = true;
        }

        if (enableTracking)
        {
            Tracking = true;
        }
    }

    private static System.Linq.Expressions.Expression<Func<Income, bool>> BuildCriteria(int id, string? createdBy, bool filtersIgnored)
        => filtersIgnored
            ? i => i.Id == id && !i.IsDeleted && (createdBy == null || i.CreatedBy == createdBy)
            : i => i.Id == id && (createdBy == null || i.CreatedBy == createdBy);
}
