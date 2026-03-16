using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BudgetEstimations.Commands.Delete;

public sealed record DeleteBudgetEstimationCommand(int Id) : ICommand;
