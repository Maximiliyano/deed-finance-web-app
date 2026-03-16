using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IDebtRepository
{
    Task<IEnumerable<Debt>> GetAllAsync(ISpecification<Debt> specification, CancellationToken cancellationToken = default);

    Task<Debt?> GetAsync(ISpecification<Debt> specification, CancellationToken cancellationToken = default);

    void Create(Debt debt);

    void Update(Debt debt);

    void Delete(Debt debt);

    Task<bool> AnyAsync(ISpecification<Debt> specification, CancellationToken cancellationToken = default);
}
