namespace Deed.Application.Debts.Requests;

public sealed record CreateDebtRequest(
    string Item,
    decimal Amount,
    string Currency,
    string Source,
    string Recipient,
    DateTimeOffset BorrowedAt,
    DateTimeOffset? DeadlineAt,
    string? Note,
    int? CapitalId);
