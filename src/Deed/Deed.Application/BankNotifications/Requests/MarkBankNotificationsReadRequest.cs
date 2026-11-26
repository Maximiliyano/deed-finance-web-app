namespace Deed.Application.BankNotifications.Requests;

public sealed record MarkBankNotificationsReadRequest(IReadOnlyList<int>? Ids);
