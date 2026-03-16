using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Incomes.Specifications;

internal sealed class IncomesByQuerySpecification
    : BaseSpecification<Income>
{
    public IncomesByQuerySpecification(string createdBy)
        : base(i => i.CreatedBy == createdBy)
    {
    }
}
