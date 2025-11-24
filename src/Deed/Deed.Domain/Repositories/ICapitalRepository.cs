using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Domain.Repositories;

public interface ICapitalRepository
{
    Task<IEnumerable<Capital>> GetAllAsync(ISpecification<Capital> specification);

    Task<Capital?> GetAsync(ISpecification<Capital> specification);

    void Create(Capital capital);

    void Update(Capital capital);

    Task<bool> PatchIncludeInTotalAsync(
        int id,
        bool includeInTotal,
        CancellationToken cancellationToken);

    Task<bool> PatchSavingsOnlyAsync(
        int id,
        bool onlyForSavings,
        CancellationToken cancellationToken);

    Task UpdateOrderIndexesAsync(IList<(int Id, int OrderIndex)> capitals, CancellationToken cancellationToken);

    void Delete(Capital capital);

    Task<bool> AnyAsync(ISpecification<Capital> specification);
}
