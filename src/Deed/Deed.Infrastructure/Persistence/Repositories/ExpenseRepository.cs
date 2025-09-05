using Deed.Application.Abstractions.Data;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Deed.Infrastructure.Persistence.Repositories;

internal sealed class ExpenseRepository(
    IDeedDbContext context)
    : GeneralRepository<Expense>(context), IExpenseRepository
{
    public async Task<Expense?> GetAsync(ISpecification<Expense> specification)
        => await base.GetAsync(specification);

    public async Task<IEnumerable<Expense>> GetAllAsync(int? capitalId)
    {
        var queries = DbContext.Expenses
            .Include(e => e.Capital)
            .Include(e => e.Category)
            .AsNoTracking()
            .AsSplitQuery();

        if (capitalId is not null)
        {
            queries = queries.Where(e => e.CapitalId == capitalId);
        }

        return await queries
            .Select(e => new Expense(e.Id)
            {
                Amount = e.Amount,
                PaymentDate = e.PaymentDate,
                CapitalId = e.CapitalId,
                Capital = new Capital(e.Capital!.Id)
                {
                    Name = e.Capital.Name,
                    Currency = e.Capital.Currency,
                    Balance = e.Capital.Balance
                },
                CategoryId = e.CategoryId,
                Category = new Category(e.Category!.Id)
                {
                    Name = e.Category.Name,
                    Type = e.Category.Type,
                    PlannedPeriodAmount = e.Category.PlannedPeriodAmount
                },
                Purpose = e.Purpose,
                CreatedAt = e.CreatedAt,
                CreatedBy = e.CreatedBy,
                UpdatedAt = e.UpdatedAt,
                UpdatedBy = e.UpdatedBy
            })
            .ToListAsync();
    }

    public void Create(Expense expense)
        => base.Create(expense);

    public void Update(Expense expense)
        => base.Update(expense);

    public void Delete(Expense expense)
        => base.Delete(expense);

    public async Task<bool> AnyAsync(ISpecification<Expense> specification)
        => await base.AnyAsync(specification);
}
