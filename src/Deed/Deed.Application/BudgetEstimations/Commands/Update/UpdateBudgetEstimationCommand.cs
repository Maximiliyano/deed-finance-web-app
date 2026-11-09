using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BudgetEstimations.Commands.Update;

public sealed record UpdateBudgetEstimationCommand(
    int Id,
    string Description,
    decimal BudgetAmount,
    string BudgetCurrency,
    int? CapitalId,
    bool IsCompleted)
    : ICommand;
