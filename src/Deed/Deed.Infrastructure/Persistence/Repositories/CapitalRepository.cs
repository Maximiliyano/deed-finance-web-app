using System.Globalization;
using Deed.Application.Abstractions.Data;
using Deed.Application.Capitals.Requests;
using Deed.Domain.Constants;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Deed.Infrastructure.Persistence.Constants;
using Deed.Infrastructure.Persistence.DataSeed;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class CapitalRepository(IDeedDbContext context)
    : GeneralRepository<Capital>(context), ICapitalRepository
{
    public async Task UpdateOrderIndexesAsync(IList<(int Id, int OrderIndex)> capitals, CancellationToken cancellationToken)
    {
        var ids = capitals.Select(c => c.Id).ToArray();
        var entries = await DbContext.Capitals
            .IgnoreAutoIncludes()
            .Where(c => ids.Contains(c.Id))
            .ToListAsync(cancellationToken);

        foreach (var entity in entries)
        {
            entity.OrderIndex = capitals.First(c => c.Id == entity.Id).OrderIndex;
        }
    }
}
