using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Domain.Repositories;

public interface IBankNotificationRepository
{
    Task<IEnumerable<BankNotification>> GetAllAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default);

    Task<int> CountAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default);

    Task<BankNotification?> GetAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BankTransactionTotal>> GetTotalsAsync(ISpecification<BankNotification> specification, BankSummaryPeriod period, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BankCounterpartyTotal>> GetCounterpartyTotalsAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetExternalIdsAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default);

    Task<DateTimeOffset?> GetOldestAsync(ISpecification<BankNotification> specification, CancellationToken cancellationToken = default);

    Task<int> MarkReadAsync(string createdBy, IReadOnlyCollection<int>? ids, CancellationToken cancellationToken = default);

    Task<int> DeleteByConnectionsAsync(string createdBy, IReadOnlyCollection<int> connectionIds, CancellationToken cancellationToken = default);

    void CreateRange(IEnumerable<BankNotification> notifications);
}
