using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IGoalRepository
{
    Task<IEnumerable<Goal>> GetAllAsync(ISpecification<Goal> specification, CancellationToken cancellationToken = default);

    Task<Goal?> GetAsync(ISpecification<Goal> specification, CancellationToken cancellationToken = default);

    void Create(Goal goal);

    void Update(Goal goal);

    void Delete(Goal goal);

    Task<bool> AnyAsync(ISpecification<Goal> specification, CancellationToken cancellationToken = default);

    Task<int> CountAsync(ISpecification<Goal> specification, CancellationToken cancellationToken = default);

    Task UpdateOrderIndexesAsync(IList<(int Id, int OrderIndex)> goals, string createdBy, CancellationToken cancellationToken = default);
}
