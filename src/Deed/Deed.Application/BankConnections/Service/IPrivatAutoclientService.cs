using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Service;

public interface IPrivatAutoclientService
{
    Task<Result<IReadOnlyList<BankTransaction>>> GetTransactionsAsync(
        string token,
        string iban,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);
}
