using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IBankNotificationRepository
{
    Task<IEnumerable<BankNotification>> GetAllAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default);

    Task<int> CountAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default);

    void CreateRange(IEnumerable<BankNotification> notifications);
}
