using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface IBankNotificationRepository
{
    Task<IEnumerable<BankNotification>> GetAllAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default);

    Task<int> CountAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BankTransactionTotal>> GetTotalsAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BankCounterpartyTotal>> GetCounterpartyTotalsAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default);

    Task<DateTimeOffset?> GetOldestAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default);

    Task<int> DeleteByConnectionsAsync(string createdBy, IReadOnlyCollection<int> connectionIds, CancellationToken cancellationToken = default);

    void CreateRange(IEnumerable<BankNotification> notifications);
}
