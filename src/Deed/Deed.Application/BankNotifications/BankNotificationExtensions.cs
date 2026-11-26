using Deed.Application.BankNotifications.Responses;
using Deed.Domain.Entities;

namespace Deed.Application.BankNotifications;

internal static class BankNotificationExtensions
{
    internal static BankNotificationResponse ToResponse(this BankNotification notification)
        => new(
            notification.Id,
            notification.Kind.ToString(),
            notification.Type.ToString(),
            notification.Direction.ToString(),
            notification.Amount,
            notification.Currency,
            notification.Counterparty,
            notification.Purpose,
            notification.AccountLabel,
            notification.OccurredAt,
            notification.IsRead,
            notification.ImportStatus.ToString(),
            notification.BankConnectionId);

    internal static IReadOnlyList<BankNotificationResponse> ToResponses(this IEnumerable<BankNotification> notifications)
        => [.. notifications.Select(n => n.ToResponse())];
}
