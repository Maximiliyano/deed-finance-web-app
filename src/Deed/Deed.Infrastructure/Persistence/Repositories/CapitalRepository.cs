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
    public async Task<IEnumerable<Capital>> GetAllAsync(string? searchTerm = null, string? sortBy = null, string? sortDirection = null)
    {
        var query = DbContext.Capitals
            .AsSplitQuery()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.Name.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase));
        }

        query = ApplySorting(query, sortBy, sortDirection);

        return await query.ToListAsync();
    }

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

    private static IQueryable<Capital> ApplySorting(
        IQueryable<Capital> query,
        string? sortBy,
        string? sortDirection)
    {
        bool asc = sortDirection?.ToLower(CultureInfo.CurrentCulture) == "asc";

        return sortBy?.ToLower(CultureInfo.CurrentCulture) switch
        {
            SortKeysConstants.Name => asc ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
            SortKeysConstants.Balance => asc ? query.OrderBy(c => c.Balance) : query.OrderByDescending(c => c.Balance),
            SortKeysConstants.Expenses => asc ? query.OrderBy(c => c.TotalExpense) : query.OrderByDescending(c => c.TotalExpense),
            SortKeysConstants.Incomes => asc ? query.OrderBy(c => c.TotalIncome) : query.OrderByDescending(c => c.TotalIncome),
            SortKeysConstants.TransfersIn => asc ? query.OrderBy(c => c.TotalTransferIn) : query.OrderByDescending(c => c.TotalTransferIn),
            SortKeysConstants.TransfersOut => asc ? query.OrderBy(c => c.TotalTransferOut) : query.OrderByDescending(c => c.TotalTransferOut),
            _ => query.OrderBy(c => c.OrderIndex)
        };
    }
}
