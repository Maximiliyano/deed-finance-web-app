using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BankConnections.Commands.ConnectPrivat;

public sealed record ConnectPrivatCommand(string Token, string Iban, string? Label = null) : ICommand;
