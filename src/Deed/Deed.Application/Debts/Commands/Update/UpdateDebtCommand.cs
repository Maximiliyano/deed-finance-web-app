using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Debts.Commands.Update;

public sealed record UpdateDebtCommand(
    int Id,
    string? Item = null,
    decimal? Amount = null,
    string? Currency = null,
    string? Source = null,
    string? Recipient = null,
    DateTimeOffset? BorrowedAt = null,
    DateTimeOffset? DeadlineAt = null,
    string? Note = null,
    bool? IsPaid = null,
    int? PayFromCapitalId = null)
    : ICommand;
