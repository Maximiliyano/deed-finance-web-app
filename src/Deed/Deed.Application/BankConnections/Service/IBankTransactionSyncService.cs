namespace Deed.Application.BankConnections.Service;

public interface IBankTransactionSyncService
{
    Task<int> SyncAsync(CancellationToken cancellationToken);

    Task<int> SyncUserAsync(string createdBy, CancellationToken cancellationToken);
}
