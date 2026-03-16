using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Expenses.Specifications;

internal sealed class ExpenseByIdSpecification : BaseSpecification<Expense>
{
    public ExpenseByIdSpecification(int id, string? createdBy = null, bool includeCapital = false, bool includeCategory = false, bool includeTags = false, bool enableTracking = false)
        : base(x => x.Id == id && (createdBy == null || x.CreatedBy == createdBy))
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
            AddInclude(i => i.Tags);
        }
        if (enableTracking)
        {
            Tracking = true;
        }
    }
}
