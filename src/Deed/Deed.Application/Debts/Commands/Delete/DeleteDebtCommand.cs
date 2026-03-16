using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Debts.Commands.Delete;

public sealed record DeleteDebtCommand(int Id) : ICommand;
