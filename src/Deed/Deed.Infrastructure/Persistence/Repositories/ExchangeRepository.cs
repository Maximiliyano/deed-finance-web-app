using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class ExchangeRepository(IDeedDbContext context)
    : GeneralRepository<Exchange>(context), IExchangeRepository
{
    public new async Task<IEnumerable<Exchange>> GetAllAsync()
        => await DbContext.Set<Exchange>()
            .AsNoTracking()
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
}
