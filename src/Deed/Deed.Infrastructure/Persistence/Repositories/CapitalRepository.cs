using System.Globalization;
using Deed.Application.Abstractions.Data;
using Deed.Application.Capitals.Requests;
using Deed.Domain.Constants;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class CapitalRepository(
    IDeedDbContext context)
    : GeneralRepository<Capital>(context), ICapitalRepository
{
    public async Task<IEnumerable<Capital>> GetAllAsync(string? searchTerm = null, string? sortBy = null, string? sortDirection = null)
    {
        var query = DbContext.Capitals
            .Where(c => !(c.IsDeleted ?? false))
            .AsSplitQuery()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.Name.ToLower().Contains(searchTerm.ToLower()));
        }

        query = ApplySorting(query, sortBy, sortDirection);

        return await query.ToListAsync();
    }

    public new async Task<Capital?> GetAsync(ISpecification<Capital> specification)
        => await base.GetAsync(specification);

    public new void Create(Capital capital)
        => base.Create(capital);

    public new void Update(Capital capital)
        => base.Update(capital);

    public async Task UpdateOrderIndexes(IEnumerable<(int Id, int OrderIndex)> capitals)
    {
        using var transaction = await DbContext.BeginTransactionAsync();

        foreach (var (Id, OrderIndex) in capitals)
        {
            await DbContext.Capitals
                .Where(c => c.Id == Id)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(c => c.OrderIndex, OrderIndex));
        }

        await transaction.CommitAsync();
    }

    public new void Delete(Capital capital)
        => base.Delete(capital);

    public new async Task<bool> AnyAsync(ISpecification<Capital> specification)
        => await base.AnyAsync(specification);

    private static IQueryable<Capital> ApplySorting(
        IQueryable<Capital> query,
        string? sortBy,
        string? sortDirection)
    {
        bool asc = sortDirection?.ToLower() == "asc";

        return sortBy?.ToLower() switch
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
