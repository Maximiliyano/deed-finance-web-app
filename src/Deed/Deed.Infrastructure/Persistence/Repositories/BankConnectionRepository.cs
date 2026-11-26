using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class BankConnectionRepository(IDeedDbContext context)
    : GeneralRepository<BankConnection>(context), IBankConnectionRepository
{
    public async Task<BankBackfillState?> GetBackfillStateAsync(int id, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new BankBackfillState(c.BackfillCursor, c.IsExcluded))
            .FirstOrDefaultAsync(cancellationToken);
}
