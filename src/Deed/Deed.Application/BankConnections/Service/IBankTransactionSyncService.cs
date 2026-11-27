using Deed.Application.BankConnections.Responses;
using Deed.Domain.Entities;

namespace Deed.Application.BankConnections.Service;

public interface IBankTransactionSyncService
{
    Task<int> SyncAsync(CancellationToken cancellationToken);

    Task<int> SyncUserAsync(string createdBy, CancellationToken cancellationToken);

    Task<BankHistoryImportResponse> ImportHistoryAsync(string createdBy, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    Task<int> BackfillAsync(CancellationToken cancellationToken);

    Task<int> ImportStatementAsync(BankConnection connection, IReadOnlyCollection<BankTransaction> transactions, CancellationToken cancellationToken);

    Task<int> StageHistoryAsync(string createdBy, string label, int? connectionId, string? currency, IReadOnlyCollection<BankTransaction> transactions, CancellationToken cancellationToken);
}
