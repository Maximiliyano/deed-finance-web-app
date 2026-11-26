using Deed.Application.BankNotifications.Responses;
using Deed.Domain.Entities;

namespace Deed.Application.BankNotifications;

internal static class BankNotificationExtensions
{
    internal static BankNotificationResponse ToResponse(this BankNotification notification)
        => new(
            notification.Id,
            notification.Kind.ToString(),
            notification.Direction.ToString(),
            notification.Amount,
            notification.Currency,
            notification.Counterparty,
            notification.Purpose,
            notification.OccurredAt,
            notification.IsRead);

    internal static IReadOnlyList<BankNotificationResponse> ToResponses(this IEnumerable<BankNotification> notifications)
        => [.. notifications.Select(n => n.ToResponse())];
}
