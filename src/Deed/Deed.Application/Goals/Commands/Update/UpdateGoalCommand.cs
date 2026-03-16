using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Enums;

namespace Deed.Application.Goals.Commands.Update;

public sealed record UpdateGoalCommand(
    int Id,
    string Title,
    decimal TargetAmount,
    CurrencyType Currency,
    decimal CurrentAmount,
    DateTimeOffset? Deadline,
    string? Note,
    bool IsCompleted)
    : ICommand;
