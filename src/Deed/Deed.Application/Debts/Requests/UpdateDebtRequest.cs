namespace Deed.Application.Debts.Requests;

public sealed record UpdateDebtRequest(
    string Item,
    decimal Amount,
    string Currency,
    string Source,
    string Recipient,
    DateTimeOffset BorrowedAt,
    DateTimeOffset? DeadlineAt,
    string? Note,
    bool IsPaid,
    int? PayFromCapitalId
);
