namespace Deed.Application.Goals.Requests;

public sealed record UpdateGoalOrdersRequest(
    IEnumerable<GoalOrder> Goals
);
