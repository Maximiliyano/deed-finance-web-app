namespace Deed.Application.BankNotifications.Responses;

public sealed record BankNotificationsResponse(
    int UnreadCount,
    IReadOnlyList<BankNotificationResponse> Items);
