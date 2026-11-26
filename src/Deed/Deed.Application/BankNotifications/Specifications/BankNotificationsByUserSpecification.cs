using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.BankNotifications.Specifications;

internal sealed class BankNotificationsByUserSpecification : BaseSpecification<BankNotification>
{
    public BankNotificationsByUserSpecification(
        string createdBy,
        bool unreadOnly = false,
        IReadOnlyCollection<int>? ids = null,
        int? take = null,
        bool tracking = false)
        : base(n =>
            n.CreatedBy == createdBy &&
            (!unreadOnly || !n.IsRead) &&
            (ids == null || ids.Contains(n.Id)))
    {
        ApplyOrderByDescending(n => n.OccurredAt);
        Take = take;
        Tracking = tracking;
    }
}
