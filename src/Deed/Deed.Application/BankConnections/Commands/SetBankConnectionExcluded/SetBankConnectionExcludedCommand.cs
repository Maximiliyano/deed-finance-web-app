using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BankConnections.Commands.SetBankConnectionExcluded;

public sealed record SetBankConnectionExcludedCommand(int Id, bool Excluded) : ICommand;
