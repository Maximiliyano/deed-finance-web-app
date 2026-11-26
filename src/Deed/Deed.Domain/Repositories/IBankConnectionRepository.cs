using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IBankConnectionRepository
{
    Task<IEnumerable<BankConnection>> GetAllAsync(ISpecification<BankConnection> specification, CancellationToken cancellationToken = default);

    Task<BankConnection?> GetAsync(ISpecification<BankConnection> specification, CancellationToken cancellationToken = default);

    Task<BankBackfillState?> GetBackfillStateAsync(int id, CancellationToken cancellationToken = default);

    void Create(BankConnection connection);

    void Delete(BankConnection connection);
}
