namespace Deed.Application.BankConnections.Service;

public interface IBankTransactionSyncService
{
    Task<int> SyncAsync(CancellationToken cancellationToken);

    Task<int> SyncUserAsync(string createdBy, CancellationToken cancellationToken);

    Task<int> ImportHistoryAsync(string createdBy, CancellationToken cancellationToken);

    Task<int> StageHistoryAsync(string createdBy, string label, IReadOnlyCollection<BankTransaction> transactions, CancellationToken cancellationToken);
}
