using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Enums;

namespace Deed.Application.UtilityBills.Commands.Update;

public sealed record UpdateUtilityBillCommand(
    int Id,
    string Name,
    decimal EstimatedAmount,
    CurrencyType Currency,
    int DueDayOfMonth,
    int CapitalId,
    int CategoryId,
    bool IsActive)
    : ICommand;
