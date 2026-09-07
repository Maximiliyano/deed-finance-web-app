using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.UtilityBills.Commands.Delete;

public sealed record DeleteUtilityBillCommand(int Id) : ICommand;
