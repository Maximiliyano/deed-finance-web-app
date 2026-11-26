using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Tags.Commands.Create;

public sealed record CreateExpenseTagCommand(
    int ExpenseId,
    string Name
): ICommand<int>;
