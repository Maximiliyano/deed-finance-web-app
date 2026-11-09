using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Debts.Commands.Create;

public sealed record CreateDebtCommand(
    string Item,
    decimal Amount,
    string Currency,
    string Source,
    string Recipient,
    DateTimeOffset BorrowedAt,
    DateTimeOffset? DeadlineAt,
    string? Note,
    int? CapitalId)
    : ICommand<int>;
