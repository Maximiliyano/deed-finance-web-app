namespace Deed.Application.Expenses.Requests;

public sealed record CreateExpenseRequest(
    int CapitalId,
    int CategoryId,
    decimal Amount,
    DateTimeOffset PaymentDate,
    string? Purpose);
