using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Deed.Application.Incomes.Specifications;

internal sealed class IncomeByIdSpecification : BaseSpecification<Income>
{
    public IncomeByIdSpecification(int id, string? createdBy = null, bool includeTags = false, bool enableTracking = false)
        : base(i => i.Id == id && (createdBy == null || i.CreatedBy == createdBy))
    {
        AddInclude(i => i.Capital!);
        AddInclude(i => i.Category!);

        if (includeTags)
        {
            Includes.Add(q => q.Include(i => i.Tags).ThenInclude(t => t.Tag));
        }

        if (enableTracking)
        {
            Tracking = true;
        }
    }
}
