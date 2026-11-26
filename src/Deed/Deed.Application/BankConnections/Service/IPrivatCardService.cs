using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Service;

public interface IPrivatCardService
{
    Task<Result<IReadOnlyList<BankTransaction>>> GetTransactionsAsync(
        PrivatCardCredentials credentials,
        string cardNumber,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);
}
