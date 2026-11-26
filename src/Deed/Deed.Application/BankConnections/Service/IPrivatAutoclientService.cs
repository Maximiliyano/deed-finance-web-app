using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Service;

public interface IPrivatAutoclientService : IBankStatementProvider
{
    Task<Result<IReadOnlyList<PrivatAccount>>> GetAccountsAsync(string token, CancellationToken cancellationToken);
}
