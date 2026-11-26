namespace Deed.Application.Goals.Requests;

public sealed record CreateGoalRequest(
    string Title,
    decimal TargetAmount,
    string Currency,
    decimal CurrentAmount,
    DateTimeOffset? Deadline,
    string? Note
);
