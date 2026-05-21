using Deed.Application.Abstractions.Messaging;
using Deed.Application.Debts.Requests;

namespace Deed.Application.Debts.Commands.UpdateOrders;

public sealed record UpdateDebtOrdersCommand(
    IEnumerable<DebtOrder> Debts)
    : ICommand;
