using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class ExpenseRepository(IDeedDbContext context)
    : GeneralRepository<Expense>(context), IExpenseRepository
{
    public async Task<IEnumerable<Expense>> GetAllAsync(int? capitalId)
    {
        var queries = DbContext.Expenses
            .Include(e => e.Capital)
            .Include(e => e.Category)
            .AsSplitQuery();

        if (capitalId.HasValue)
        {
            queries = queries.Where(e => e.CapitalId == capitalId.Value);
        }

        return await queries.ToListAsync().ConfigureAwait(false);
    }
}
