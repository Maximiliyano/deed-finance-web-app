using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(IDeedDbContext context)
    : GeneralRepository<Category>(context), ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetAllAsync(CategoryType? type = null, IEnumerable<int>? ids = null, bool? includeDeleted = null, bool tracking = false)
    { // TODO write into specification
        var queryable = DbContext.Categories.AsQueryable();

        if (includeDeleted.HasValue && includeDeleted.Value)
        {
            queryable = queryable.IgnoreQueryFilters();
        }

        if (type.HasValue)
        {
            queryable = queryable.Where(c => c.Type == type.Value);
        }

        if (ids != null)
        {
            queryable = queryable.Where(c => ids.Contains(c.Id));
        }

        if (!tracking)
        {
            queryable = queryable.AsNoTracking();
        }

        return await queryable.ToListAsync();
    }
}
