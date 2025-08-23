using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface ICapitalRepository
{
    Task<IEnumerable<Capital>> GetAllAsync(string? searchTerm, string? sortBy = null, string? sortDirection = null);

    Task<Capital?> GetAsync(ISpecification<Capital> specification);

    void Create(Capital capital);

    void Update(Capital capital);

    Task UpdateOrderIndexes(IEnumerable<(int Id, int OrderIndex)> capitals);

    void Delete(Capital capital);

    Task<bool> AnyAsync(ISpecification<Capital> specification);
}
