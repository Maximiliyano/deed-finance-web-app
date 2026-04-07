using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface ITransferRepository
{
    Task<Transfer?> GetAsync(ISpecification<Transfer> specification, CancellationToken cancellationToken = default);

    Task<IEnumerable<Transfer>> GetAllAsync(ISpecification<Transfer> specification, CancellationToken cancellationToken = default);

    void Create(Transfer transfer);

    void Delete(Transfer transfer);

    Task<bool> AnyAsync(ISpecification<Transfer> specification, CancellationToken cancellationToken = default);

    Task<int> CountAsync(ISpecification<Transfer> specification, CancellationToken cancellationToken = default);
}
