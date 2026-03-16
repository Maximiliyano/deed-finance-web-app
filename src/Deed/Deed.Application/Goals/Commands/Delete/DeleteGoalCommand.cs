using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Goals.Commands.Delete;

public sealed record DeleteGoalCommand(int Id) : ICommand;
