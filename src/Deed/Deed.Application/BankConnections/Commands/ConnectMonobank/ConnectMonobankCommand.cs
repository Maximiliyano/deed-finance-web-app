using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BankConnections.Commands.ConnectMonobank;

public sealed record ConnectMonobankCommand(string Token, string AccountId, string? Iban, string? Label = null, string? Currency = null) : ICommand;
