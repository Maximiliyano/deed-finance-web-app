using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Expenses.Commands.Create;

public sealed record CreateExpenseCommand(
    int CapitalId,
    int CategoryId,
    decimal Amount,
    DateTimeOffset PaymentDate,
    string? Purpose,
    List<string> TagNames
) : ICommand<int>;
