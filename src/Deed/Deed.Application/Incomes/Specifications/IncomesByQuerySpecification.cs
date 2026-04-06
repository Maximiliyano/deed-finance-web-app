using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Deed.Application.Incomes.Specifications;

internal sealed class IncomesByQuerySpecification
    : BaseSpecification<Income>
{
    public IncomesByQuerySpecification(string createdBy)
        : base(i => i.CreatedBy == createdBy)
    {
        Includes.Add(i => i.Include(t => t.Tags).ThenInclude(it => it.Tag));
    }
}
