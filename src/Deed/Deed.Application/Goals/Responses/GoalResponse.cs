namespace Deed.Application.Goals.Responses;

public sealed record GoalResponse(
    int Id,
    string Title,
    decimal TargetAmount,
    string Currency,
    decimal CurrentAmount,
    DateTimeOffset? Deadline,
    string? Note,
    bool IsCompleted,
    int OrderIndex
);
