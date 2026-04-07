using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IBudgetEstimationRepository
{
    Task<IEnumerable<BudgetEstimation>> GetAllAsync(ISpecification<BudgetEstimation> specification, CancellationToken cancellationToken = default);

    Task<BudgetEstimation?> GetAsync(ISpecification<BudgetEstimation> specification, CancellationToken cancellationToken = default);

    void Create(BudgetEstimation estimation);

    void Update(BudgetEstimation estimation);

    void Delete(BudgetEstimation estimation);

    Task<bool> AnyAsync(ISpecification<BudgetEstimation> specification, CancellationToken cancellationToken = default);

    Task<int> CountAsync(ISpecification<BudgetEstimation> specification, CancellationToken cancellationToken = default);
}
