using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Deed.Application.UtilityBills.Specifications;

internal sealed class UtilityBillsByUserSpecification : BaseSpecification<UtilityBill>
{
    public UtilityBillsByUserSpecification(string createdBy, bool includeRelations = false, bool currentMonthPaymentsOnly = false)
        : base(b => b.CreatedBy == createdBy)
    {
        if (includeRelations)
        {
            AddInclude(b => b.Capital!);
            AddInclude(b => b.Category);

            if (currentMonthPaymentsOnly)
            {
                var now = DateTimeOffset.UtcNow;
                var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                var monthEnd = monthStart.AddMonths(1);

                Includes.Add(q => q.Include(b =>
                    b.Payments.Where(p => !p.IsDeleted && p.PaymentDate >= monthStart && p.PaymentDate < monthEnd)));
            }
            else
            {
                AddInclude(b => b.Payments);
            }
        }

        ApplyOrderBy(b => b.DueDayOfMonth);
    }
}
