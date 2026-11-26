using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Service;

public interface IMonobankService : IBankStatementProvider
{
    Task<Result<IReadOnlyList<MonobankAccount>>> GetAccountsAsync(string token, CancellationToken cancellationToken);
}
