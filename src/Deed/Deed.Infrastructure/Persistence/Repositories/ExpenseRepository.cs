using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class ExpenseRepository(IDeedDbContext context)
    : GeneralRepository<Expense>(context), IExpenseRepository
{
    public async Task<IReadOnlyDictionary<int, decimal>> GetLatestMeterReadingsAsync(IReadOnlyCollection<int> utilityBillIds, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default)
    {
        var rows = await DbSet
            .AsNoTracking()
            .Where(e => e.UtilityBillId != null
                && utilityBillIds.Contains(e.UtilityBillId.Value)
                && e.MeterReading != null
                && e.PaymentDate >= from
                && e.PaymentDate < to)
            .OrderByDescending(e => e.PaymentDate)
            .ThenByDescending(e => e.Id)
            .Select(e => new { BillId = e.UtilityBillId!.Value, Reading = e.MeterReading!.Value })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => r.BillId)
            .ToDictionary(g => g.Key, g => g.First().Reading);
    }

    public async Task<IReadOnlyList<CategoryAmount>> GetTotalsByCategoryAsync(string createdBy, IReadOnlyCollection<int>? capitalIds, DateTimeOffset? periodStart, DateTimeOffset? periodEnd, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Where(e => e.CreatedBy == createdBy
                && (capitalIds == null || capitalIds.Contains(e.CapitalId))
                && (periodStart == null || e.PaymentDate >= periodStart)
                && (periodEnd == null || e.PaymentDate < periodEnd))
            .GroupBy(e => new { e.CategoryId, e.Category.Name, e.Capital!.Currency })
            .Select(g => new CategoryAmount(g.Key.CategoryId, g.Key.Name, g.Key.Currency, g.Sum(e => e.Amount)))
            .ToListAsync(cancellationToken);
}
