using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BankConnections.Commands.SyncBankConnections;

public sealed record SyncBankConnectionsCommand : ICommand<int>;
