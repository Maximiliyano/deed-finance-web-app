using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(IDeedDbContext context)
    : GeneralRepository<Category>(context), ICategoryRepository
{
    public async Task<bool> HasReferencesAsync(int id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Where(c => c.Id == id)
            .AnyAsync(c => c.Expenses.Any() || c.Incomes.Any(), cancellationToken);
}
