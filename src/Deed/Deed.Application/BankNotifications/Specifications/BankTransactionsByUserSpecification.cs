using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Application.BankNotifications.Specifications;

internal sealed class BankTransactionsByUserSpecification : BaseSpecification<BankNotification>
{
    public BankTransactionsByUserSpecification(
        string createdBy,
        BankTransactionType? type = null,
        int? skip = null,
        int? take = null,
        BankImportStatus? importStatus = null,
        int? connectionId = null)
        : base(n =>
            n.CreatedBy == createdBy &&
            n.Kind == BankNotificationKind.Transaction &&
            (type == null || n.Type == type) &&
            (importStatus == null || n.ImportStatus == importStatus) &&
            (connectionId == null || n.BankConnectionId == connectionId))
    {
        if (take is null)
        {
            return;
        }

        ApplyOrderByDescending(n => n.OccurredAt);
        ApplyThenByDescending(n => n.Id);
        Skip = skip;
        Take = take;
    }
}
