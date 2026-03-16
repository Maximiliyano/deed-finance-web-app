using Deed.Application.Goals.Commands.Create;
using Deed.Application.Goals.Commands.Update;
using Deed.Application.Goals.Responses;
using Deed.Domain.Entities;

namespace Deed.Application.Goals;

internal static class GoalExtensions
{
    internal static GoalResponse ToResponse(this Goal goal)
        => new(
            goal.Id,
            goal.Title,
            goal.TargetAmount,
            goal.Currency.ToString(),
            goal.CurrentAmount,
            goal.Deadline,
            goal.Note,
            goal.IsCompleted,
            goal.OrderIndex
        );

    internal static IEnumerable<GoalResponse> ToResponses(this IEnumerable<Goal> goals)
        => goals.Select(g => g.ToResponse());

    internal static Goal ToEntity(this CreateGoalCommand cmd)
        => new()
        {
            Title = cmd.Title.Trim(),
            TargetAmount = cmd.TargetAmount,
            Currency = cmd.Currency,
            CurrentAmount = cmd.CurrentAmount,
            Deadline = cmd.Deadline,
            Note = cmd.Note?.Trim(),
        };

    internal static void ApplyUpdate(this Goal goal, UpdateGoalCommand cmd)
    {
        goal.Title = cmd.Title.Trim();
        goal.TargetAmount = cmd.TargetAmount;
        goal.Currency = cmd.Currency;
        goal.CurrentAmount = cmd.CurrentAmount;
        goal.Deadline = cmd.Deadline;
        goal.Note = cmd.Note?.Trim();
        goal.IsCompleted = cmd.IsCompleted;
    }
}
