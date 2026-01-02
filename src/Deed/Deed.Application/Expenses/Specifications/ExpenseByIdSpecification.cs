using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Expenses.Specifications;

internal sealed class ExpenseByIdSpecification : BaseSpecification<Expense>
{
    public ExpenseByIdSpecification(int id, bool includeCapital = false, bool includeCategory = false, bool enableTracking = false)
        : base(x => x.Id == id)
    {
        if (includeCapital)
        {
            AddInclude(i => i.Capital);
        }
        if (includeCategory)
        {
            AddInclude(i => i.Category);
        }
        if (enableTracking)
        {
            Tracking = true;
        }
    }
}
