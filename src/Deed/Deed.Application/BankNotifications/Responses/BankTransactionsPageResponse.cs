namespace Deed.Application.BankNotifications.Responses;

public sealed record BankTransactionsPageResponse(
    int Total,
    int Page,
    int PageSize,
    IReadOnlyList<BankNotificationResponse> Items);
