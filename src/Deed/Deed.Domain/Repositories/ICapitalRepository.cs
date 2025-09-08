using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface ICapitalRepository
{
    Task<IEnumerable<Capital>> GetAllAsync(ISpecification<Capital> specification);

    Task<Capital?> GetAsync(ISpecification<Capital> specification);

    void Create(Capital capital);

    void Update(Capital capital);

    Task UpdateOrderIndexesAsync(IList<(int Id, int OrderIndex)> capitals, CancellationToken cancellationToken);

    void Delete(Capital capital);

    Task<bool> AnyAsync(ISpecification<Capital> specification);
}
