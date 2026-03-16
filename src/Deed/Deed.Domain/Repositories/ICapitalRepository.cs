using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Domain.Repositories;

public interface ICapitalRepository
{
    Task<IEnumerable<Capital>> GetAllAsync(ISpecification<Capital> specification, CancellationToken cancellationToken = default);

    Task<Capital?> GetAsync(ISpecification<Capital> specification, CancellationToken cancellationToken = default);

    void Create(Capital capital);

    void Update(Capital capital);

    Task<bool> PatchIncludeInTotalAsync(
        int id,
        bool includeInTotal,
        string createdBy,
        CancellationToken cancellationToken);

    Task<bool> PatchSavingsOnlyAsync(
        int id,
        bool onlyForSavings,
        string createdBy,
        CancellationToken cancellationToken);

    Task UpdateOrderIndexesAsync(IList<(int Id, int OrderIndex)> capitals, string createdBy, CancellationToken cancellationToken);

    void Delete(Capital capital);

    Task<bool> AnyAsync(ISpecification<Capital> specification, CancellationToken cancellationToken = default);
}
