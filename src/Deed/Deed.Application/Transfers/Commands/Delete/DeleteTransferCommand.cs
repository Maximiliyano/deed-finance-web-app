using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Transfers.Commands.Delete;

public sealed record DeleteTransferCommand(int Id) : ICommand;
