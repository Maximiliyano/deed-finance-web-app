using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IUtilityBillRepository
{
    Task<IEnumerable<UtilityBill>> GetAllAsync(ISpecification<UtilityBill> specification, CancellationToken cancellationToken = default);

    Task<UtilityBill?> GetAsync(ISpecification<UtilityBill> specification, CancellationToken cancellationToken = default);

    void Create(UtilityBill utilityBill);

    void Update(UtilityBill utilityBill);

    void Delete(UtilityBill utilityBill);

    Task<bool> AnyAsync(ISpecification<UtilityBill> specification, CancellationToken cancellationToken = default);

    Task<int> CountAsync(ISpecification<UtilityBill> specification, CancellationToken cancellationToken = default);
}
