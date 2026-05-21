using Deed.Application.Abstractions.Messaging;
using Deed.Application.BudgetEstimations.Requests;

namespace Deed.Application.BudgetEstimations.Commands.UpdateOrders;

public sealed record UpdateEstimationOrdersCommand(
    IEnumerable<EstimationOrder> Estimations)
    : ICommand;
