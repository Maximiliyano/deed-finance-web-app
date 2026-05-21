using Deed.Application.Abstractions.Messaging;
using Deed.Application.Goals.Requests;

namespace Deed.Application.Goals.Commands.UpdateOrders;

public sealed record UpdateGoalOrdersCommand(
    IEnumerable<GoalOrder> Goals)
    : ICommand;
