using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Enums;

namespace Deed.Application.Goals.Commands.Create;

public sealed record CreateGoalCommand(
    string Title,
    decimal TargetAmount,
    CurrencyType Currency,
    decimal CurrentAmount,
    DateTimeOffset? Deadline,
    string? Note)
    : ICommand<int>;
