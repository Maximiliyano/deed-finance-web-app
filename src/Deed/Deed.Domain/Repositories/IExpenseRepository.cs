using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IExpenseRepository
{
    Task<Expense?> GetAsync(ISpecification<Expense> specification, CancellationToken cancellationToken = default);

    Task<IEnumerable<Expense>> GetAllAsync(ISpecification<Expense> specification, CancellationToken cancellationToken = default);

    void Create(Expense expense);

    void Update(Expense expense);

    void Delete(Expense expense);

    Task<bool> AnyAsync(ISpecification<Expense> specification, CancellationToken cancellationToken = default);

    Task<int> CountAsync(ISpecification<Expense> specification, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, decimal>> GetLatestMeterReadingsAsync(IReadOnlyCollection<int> utilityBillIds, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryAmount>> GetTotalsByCategoryAsync(string createdBy, IReadOnlyCollection<int>? capitalIds, DateTimeOffset? periodStart, DateTimeOffset? periodEnd, CancellationToken cancellationToken = default);
}
