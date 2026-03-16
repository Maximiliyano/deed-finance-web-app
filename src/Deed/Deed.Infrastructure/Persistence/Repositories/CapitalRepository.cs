using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Deed.Infrastructure.Persistence.Constants;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class CapitalRepository(IDeedDbContext context)
    : GeneralRepository<Capital>(context), ICapitalRepository
{
    public async Task<bool> PatchIncludeInTotalAsync(int id, bool includeInTotal, string createdBy, CancellationToken cancellationToken)
    {
        return await DbSet
            .Where(c => c.Id == id && c.CreatedBy == createdBy)
            .ExecuteUpdateAsync(p => p.SetProperty(c => c.IncludeInTotal, includeInTotal), cancellationToken) > 0;
    }

    public async Task<bool> PatchSavingsOnlyAsync(int id, bool onlyForSavings, string createdBy, CancellationToken cancellationToken)
    {
        return await DbSet
            .Where(c => c.Id == id && c.CreatedBy == createdBy)
            .ExecuteUpdateAsync(p => p.SetProperty(c => c.OnlyForSavings, onlyForSavings), cancellationToken) > 0;
    }

    public async Task UpdateOrderIndexesAsync(IList<(int Id, int OrderIndex)> capitals, string createdBy, CancellationToken cancellationToken)
    {
        var ids = capitals.Select(c => c.Id).ToArray();
        var entries = await DbSet
            .Where(c => ids.Contains(c.Id) && c.CreatedBy == createdBy)
            .ToListAsync(cancellationToken);

        var orderMap = capitals.ToDictionary(c => c.Id, c => c.OrderIndex);
        foreach (var entity in entries)
        {
            entity.OrderIndex = orderMap[entity.Id];
        }
    }
}
