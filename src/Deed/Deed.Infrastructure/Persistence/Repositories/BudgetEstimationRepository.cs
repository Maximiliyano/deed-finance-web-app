using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class BudgetEstimationRepository(IDeedDbContext context)
    : GeneralRepository<BudgetEstimation>(context), IBudgetEstimationRepository
{
    public async Task UpdateOrderIndexesAsync(IList<(int Id, int OrderIndex)> estimations, string createdBy, CancellationToken cancellationToken = default)
    {
        var ids = estimations.Select(e => e.Id).ToArray();
        var entries = await DbSet
            .Where(e => ids.Contains(e.Id) && e.CreatedBy == createdBy)
            .ToListAsync(cancellationToken);

        var orderMap = estimations.ToDictionary(e => e.Id, e => e.OrderIndex);
        foreach (var entity in entries)
        {
            entity.OrderIndex = orderMap[entity.Id];
        }
    }
}
