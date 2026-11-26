using Deed.Domain.Enums;
using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Service;

public interface IBankStatementProvider
{
    BankProvider Provider { get; }

    int? MaxItemsPerRequest { get; }

    Task<Result<IReadOnlyList<BankTransaction>>> GetTransactionsAsync(
        string token,
        string account,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);
}
