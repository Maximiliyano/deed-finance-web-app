using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Enums;

namespace Deed.Application.Debts.Commands.Update;

public sealed record UpdateDebtCommand(
    int Id,
    string Item,
    decimal Amount,
    CurrencyType Currency,
    string Source,
    string Recipient,
    DateTimeOffset BorrowedAt,
    DateTimeOffset? DeadlineAt,
    string? Note,
    bool IsPaid,
    int? PayFromCapitalId)
    : ICommand;
