using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Incomes.Commands.Update;

public sealed record UpdateIncomeCommand(
    int Id,
    int? CategoryId = null,
    decimal? Amount = null,
    string? Purpose = null,
    DateTimeOffset? PaymentDate = null)
    : ICommand;
