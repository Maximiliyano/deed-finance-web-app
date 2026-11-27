using Deed.Application.Abstractions.Messaging;
using Deed.Application.BankConnections.Responses;

namespace Deed.Application.BankConnections.Commands.ConnectPrivatMail;

public sealed record ConnectPrivatMailCommand(string Iban, string? Label = null, string? Currency = null)
    : ICommand<PrivatMailConnectionResponse>;
