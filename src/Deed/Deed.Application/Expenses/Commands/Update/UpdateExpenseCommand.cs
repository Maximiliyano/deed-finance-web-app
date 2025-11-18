using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Expenses.Commands.Update;

public sealed record UpdateExpenseCommand(
    int Id,
    int? CategoryId = null,
    int? CapitalId = null,
    decimal? Amount = null,
    string? Purpose = null,
    DateTimeOffset? Date = null)
    : ICommand;
