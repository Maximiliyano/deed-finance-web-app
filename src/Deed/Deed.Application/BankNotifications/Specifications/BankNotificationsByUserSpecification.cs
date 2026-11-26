using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Application.BankNotifications.Specifications;

internal sealed class BankNotificationsByUserSpecification : BaseSpecification<BankNotification>
{
    public BankNotificationsByUserSpecification(
        string createdBy,
        bool unreadOnly = false,
        IReadOnlyCollection<int>? ids = null,
        int? take = null,
        bool tracking = false,
        DateTimeOffset? since = null,
        BankNotificationKind? kind = null,
        IReadOnlyCollection<int>? connectionIds = null)
        : base(n =>
            n.CreatedBy == createdBy &&
            (!unreadOnly || !n.IsRead) &&
            (ids == null || ids.Contains(n.Id)) &&
            (since == null || n.OccurredAt >= since) &&
            (kind == null || n.Kind == kind) &&
            (connectionIds == null || n.BankConnectionId == null || connectionIds.Contains(n.BankConnectionId.Value)))
    {
        ApplyOrderByDescending(n => n.OccurredAt);
        Take = take;
        Tracking = tracking;
    }
}
