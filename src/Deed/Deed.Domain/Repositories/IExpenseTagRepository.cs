using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IExpenseTagRepository
{
    void Create(ExpenseTag tag);

    Task<IEnumerable<ExpenseTag>> GetAllAsync(ISpecification<ExpenseTag> specification, CancellationToken cancellationToken = default);
}
