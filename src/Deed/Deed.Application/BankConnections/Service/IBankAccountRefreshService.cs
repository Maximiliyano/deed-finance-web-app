namespace Deed.Application.BankConnections.Service;

public interface IBankAccountRefreshService
{
    Task<int> RefreshAsync(CancellationToken cancellationToken);

    Task<int> RefreshUserAsync(string createdBy, CancellationToken cancellationToken);
}
