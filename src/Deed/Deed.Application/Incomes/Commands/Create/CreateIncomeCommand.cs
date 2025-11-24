using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Incomes.Commands.Create;

public sealed record CreateIncomeCommand(
    int CapitalId,
    int CategoryId,
    decimal Amount,
    DateTimeOffset PaymentDate,
    string? Purpose = null)
    : ICommand<int>;
