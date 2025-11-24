using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IExpenseRepository
{
    Task<Expense?> GetAsync(ISpecification<Expense> specification);

    Task<IEnumerable<Expense>> GetAllAsync(int? capitalId);

    void Create(Expense expense);

    void Update(Expense expense);

    void Delete(Expense expense);

    Task<bool> AnyAsync(ISpecification<Expense> specification);
}
