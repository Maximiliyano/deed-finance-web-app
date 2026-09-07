using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Enums;

namespace Deed.Application.UtilityBills.Commands.Create;

public sealed record CreateUtilityBillCommand(
    string Name,
    decimal EstimatedAmount,
    CurrencyType Currency,
    int DueDayOfMonth,
    int CapitalId,
    int CategoryId,
    bool IsActive)
    : ICommand<int>;
