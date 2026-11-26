using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Deed.Application.Expenses.Specifications;

internal sealed class ExpenseByQueriesSpecification : BaseSpecification<Expense>
{
    public ExpenseByQueriesSpecification(
        string createdBy,
        int? capitalId = null,
        DateTimeOffset? periodStart = null,
        DateTimeOffset? periodEnd = null)
        : base(e =>
            (!capitalId.HasValue || e.CapitalId == capitalId.Value) &&
            (periodStart == null || e.PaymentDate >= periodStart) &&
            (periodEnd == null || e.PaymentDate < periodEnd) &&
            e.CreatedBy == createdBy)
    {
        AddInclude(t => t.Category);

        Includes.Add(e => e.Include(t => t.Tags).ThenInclude(ta => ta.Tag));
    }
}
