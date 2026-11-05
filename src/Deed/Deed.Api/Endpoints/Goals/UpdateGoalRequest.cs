using Deed.Domain.Enums;

namespace Deed.Api.Endpoints.Goals;

internal sealed record UpdateGoalRequest(
    string Title,
    decimal TargetAmount,
    string Currency,
    decimal CurrentAmount,
    DateTimeOffset? Deadline,
    string? Note,
    bool IsCompleted);
