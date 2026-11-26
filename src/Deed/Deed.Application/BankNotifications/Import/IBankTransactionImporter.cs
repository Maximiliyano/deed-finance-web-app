using Deed.Domain.Entities;
using Deed.Domain.Results;

namespace Deed.Application.BankNotifications.Import;

public interface IBankTransactionImporter
{
    Task<Result> ImportAsync(BankConnection connection, BankNotification notification, int? categoryId, CancellationToken cancellationToken);
}
