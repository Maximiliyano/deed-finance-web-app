using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Deed.Application.Expenses.Specifications;

internal sealed class ExpenseByQueriesSpecification : BaseSpecification<Expense>
{
    public ExpenseByQueriesSpecification(string createdBy, int? capitalId = null)
        : base(e =>
            (!capitalId.HasValue || e.CapitalId == capitalId.Value) &&
            e.CreatedBy == createdBy)
    {
        AddInclude(t => t.Capital);
        AddInclude(t => t.Category);

        Includes.Add(e => e.Include(t => t.Tags).ThenInclude(ta => ta.Tag));
    }
}
