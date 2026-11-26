using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BankConnections.Commands.DisconnectBankConnection;

public sealed record DisconnectBankConnectionCommand(int Id) : ICommand;
