using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Enums;

namespace Deed.Application.BudgetEstimations.Commands.Update;

public sealed record UpdateBudgetEstimationCommand(
    int Id,
    string Description,
    decimal BudgetAmount,
    string BudgetCurrency,
    int? CapitalId,
    bool IsCompleted)
    : ICommand;
