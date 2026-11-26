using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Application.BankNotifications.Specifications;

internal sealed class BankTransactionsSummarySpecification : BaseSpecification<BankNotification>
{
    public BankTransactionsSummarySpecification(string createdBy, int? connectionId, DateTimeOffset from, DateTimeOffset to, IReadOnlyCollection<int>? connectionIds = null, bool includeUnassigned = true)
        : base(n =>
            n.CreatedBy == createdBy &&
            n.Kind == BankNotificationKind.Transaction &&
            n.Direction != BankTransactionDirection.None &&
            (connectionId == null || n.BankConnectionId == connectionId) &&
            (connectionIds == null || n.BankConnectionId == null && includeUnassigned || n.BankConnectionId != null && connectionIds.Contains(n.BankConnectionId.Value)) &&
            n.OccurredAt >= from &&
            n.OccurredAt <= to)
    {
        Tracking = false;
    }
}
