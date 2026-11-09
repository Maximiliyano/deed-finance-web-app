using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BudgetEstimations.Commands.Create;

public sealed record CreateBudgetEstimationCommand(
    string Description,
    decimal BudgetAmount,
    string BudgetCurrency,
    int? CapitalId)
    : ICommand<int>;
