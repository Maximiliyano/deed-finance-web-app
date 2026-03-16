using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Enums;

namespace Deed.Application.Debts.Commands.Create;

public sealed record CreateDebtCommand(
    string Item,
    decimal Amount,
    CurrencyType Currency,
    string Source,
    string Recipient,
    DateTimeOffset BorrowedAt,
    DateTimeOffset? DeadlineAt,
    string? Note,
    int? CapitalId)
    : ICommand<int>;
