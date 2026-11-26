using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BankConnections.Commands.DisconnectBankProvider;

public sealed record DisconnectBankProviderCommand(string Provider) : ICommand;
