namespace Deed.Application.BankNotifications.Responses;

public sealed record BankNotificationResponse(
    int Id,
    string Kind,
    string Direction,
    decimal Amount,
    string? Currency,
    string? Counterparty,
    string? Purpose,
    DateTimeOffset OccurredAt,
    bool IsRead);
