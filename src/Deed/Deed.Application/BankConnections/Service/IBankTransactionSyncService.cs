namespace Deed.Application.BankConnections.Service;

public interface IBankTransactionSyncService
{
    Task SyncAsync(CancellationToken cancellationToken);
}
