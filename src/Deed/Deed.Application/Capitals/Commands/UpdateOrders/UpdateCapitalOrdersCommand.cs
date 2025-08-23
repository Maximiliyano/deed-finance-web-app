using Deed.Application.Abstractions.Messaging;
using Deed.Application.Capitals.Requests;

namespace Deed.Application.Capitals.Commands.UpdateOrders;

public sealed record UpdateCapitalOrdersCommand(
    IEnumerable<CapitalOrder> Capitals)
    : ICommand;
