using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class ExpenseRepository(IDeedDbContext context)
    : GeneralRepository<Expense>(context), IExpenseRepository
{
    public async Task<IReadOnlyList<CategoryAmount>> GetTotalsByCategoryAsync(string createdBy, IReadOnlyCollection<int>? capitalIds, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Where(e => e.CreatedBy == createdBy && (capitalIds == null || capitalIds.Contains(e.CapitalId)))
            .GroupBy(e => new { e.CategoryId, e.Category.Name, e.Capital!.Currency })
            .Select(g => new CategoryAmount(g.Key.CategoryId, g.Key.Name, g.Key.Currency, g.Sum(e => e.Amount)))
            .ToListAsync(cancellationToken);
}
