using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Deed.Application.Expenses.Specifications;

internal sealed class ExpenseByIdSpecification : BaseSpecification<Expense>
{
    public ExpenseByIdSpecification(int id, string? createdBy = null, bool includeCapital = false, bool includeCategory = false, bool includeTags = false, bool enableTracking = false)
        : base(BuildCriteria(id, createdBy, includeTags))
    {
        if (includeCapital)
        {
            AddInclude(i => i.Capital);
        }
        if (includeCategory)
        {
            AddInclude(i => i.Category);
        }
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

    private static System.Linq.Expressions.Expression<Func<Expense, bool>> BuildCriteria(int id, string? createdBy, bool filtersIgnored)
        => filtersIgnored
            ? x => x.Id == id && !x.IsDeleted && (createdBy == null || x.CreatedBy == createdBy)
            : x => x.Id == id && (createdBy == null || x.CreatedBy == createdBy);
}
