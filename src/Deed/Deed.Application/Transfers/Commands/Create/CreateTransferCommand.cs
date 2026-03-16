using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Transfers.Commands.Create;

public sealed record CreateTransferCommand(
    int SourceCapitalId,
    int DestinationCapitalId,
    decimal Amount,
    decimal DestinationAmount)
    : ICommand<int>;
