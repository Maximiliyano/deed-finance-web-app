using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.UtilityBills.Commands.Pay;

public sealed record PayUtilityBillCommand(
    int Id,
    decimal Amount,
    DateTimeOffset? PaymentDate)
    : ICommand<int>;
