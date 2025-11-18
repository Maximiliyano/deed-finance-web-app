using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Categories.Specifications;

internal sealed class CategoryByIdSpecification
    : BaseSpecification<Category>
{
    public CategoryByIdSpecification(int id, bool includeExpenses = false, bool includeIncomes = false)
        : base(c => c.Id == id)
    {
        if (includeIncomes)
        {
            AddInclude(c => c.Incomes);
        }
        if (includeExpenses)
        {
            AddInclude(c => c.Expenses);
        }
    }
}
