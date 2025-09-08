using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Incomes.Specifications;

internal sealed class IncomeByIdSpecification
    : BaseSpecification<Income>
{
    public IncomeByIdSpecification(int id)
        : base(i => i.Id == id)
    {
        AddInclude(i => i.Capital);
        AddInclude(i => i.Category);
    }
}
