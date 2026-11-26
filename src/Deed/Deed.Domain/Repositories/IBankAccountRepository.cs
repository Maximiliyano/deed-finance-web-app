using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IBankAccountRepository
{
    Task<IEnumerable<BankAccount>> GetAllAsync(ISpecification<BankAccount> specification, CancellationToken cancellationToken = default);

    Task<BankAccount?> GetAsync(ISpecification<BankAccount> specification, CancellationToken cancellationToken = default);

    void Create(BankAccount account);

    void Delete(BankAccount account);
}
