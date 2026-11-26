using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BankConnections.Commands.ConnectBankAccount;

public sealed record ConnectBankAccountCommand(int Id, string? Label) : ICommand;
