using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Deed.Application.Incomes.Specifications;

internal sealed class IncomesByQuerySpecification
    : BaseSpecification<Income>
{
    public IncomesByQuerySpecification(
        string createdBy,
        DateTimeOffset? periodStart = null,
        DateTimeOffset? periodEnd = null)
        : base(i =>
            i.CreatedBy == createdBy &&
            (periodStart == null || i.PaymentDate >= periodStart) &&
            (periodEnd == null || i.PaymentDate < periodEnd))
    {
        Includes.Add(i => i.Include(t => t.Tags).ThenInclude(it => it.Tag));
    }
}
