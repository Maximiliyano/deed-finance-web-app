namespace Deed.Application.Debts.Responses;

public sealed record DebtResponse(
    int Id,
    string Item,
    decimal Amount,
    string Currency,
    string Source,
    string Recipient,
    DateTimeOffset BorrowedAt,
    DateTimeOffset? DeadlineAt,
    string? Note,
    bool IsPaid,
    int? CapitalId,
    string? CapitalName,
    int OrderIndex
);
