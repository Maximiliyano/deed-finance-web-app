namespace Deed.Application.Expenses.Requests;

public sealed record UpdateExpenseRequest(
    int Id,
    int? CategoryId,
    decimal? Amount,
    string? Purpose,
    DateTimeOffset? Date);
