using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class DebtRepository(IDeedDbContext context)
    : GeneralRepository<Debt>(context), IDebtRepository
{
    public async Task UpdateOrderIndexesAsync(IList<(int Id, int OrderIndex)> debts, string createdBy, CancellationToken cancellationToken = default)
    {
        var ids = debts.Select(e => e.Id).ToArray();
        var entries = await DbSet
            .Where(e => ids.Contains(e.Id) && e.CreatedBy == createdBy)
            .ToListAsync(cancellationToken);

        var orderMap = debts.ToDictionary(e => e.Id, e => e.OrderIndex);
        foreach (var entity in entries)
        {
            entity.OrderIndex = orderMap[entity.Id];
        }
    }
}
