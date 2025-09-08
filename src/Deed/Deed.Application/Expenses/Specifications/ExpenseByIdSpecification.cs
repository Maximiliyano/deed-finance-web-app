using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Expenses.Specifications;

internal sealed class ExpenseByIdSpecification : BaseSpecification<Expense>
{
    public ExpenseByIdSpecification(int id)
        : base(x => x.Id == id)
    {
        AddInclude(i => i.Capital);
        AddInclude(i => i.Category);
    }
}
