using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class IncomeRepository(IDeedDbContext context)
    : GeneralRepository<Income>(context), IIncomeRepository
{
    public async Task<IReadOnlyList<CategoryAmount>> GetTotalsByCategoryAsync(string createdBy, IReadOnlyCollection<int>? capitalIds, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Where(i => i.CreatedBy == createdBy && (capitalIds == null || capitalIds.Contains(i.CapitalId)))
            .GroupBy(i => new { i.CategoryId, i.Category!.Name, i.Capital!.Currency })
            .Select(g => new CategoryAmount(g.Key.CategoryId, g.Key.Name, g.Key.Currency, g.Sum(i => i.Amount)))
            .ToListAsync(cancellationToken);
}
