using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Enums;

namespace Deed.Application.BudgetEstimations.Commands.Create;

public sealed record CreateBudgetEstimationCommand(
    string Description,
    decimal BudgetAmount,
    CurrencyType BudgetCurrency,
    int? CapitalId)
    : ICommand<int>;
