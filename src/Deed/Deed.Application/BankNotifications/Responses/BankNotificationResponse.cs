namespace Deed.Application.BankNotifications.Responses;

public sealed record BankNotificationResponse(
    int Id,
    string Kind,
    string Type,
    string Direction,
    decimal Amount,
    string? Currency,
    string? Counterparty,
    string? Purpose,
    string? AccountLabel,
    DateTimeOffset OccurredAt,
    bool IsRead);
