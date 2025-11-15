using Deed.Application.Categories.Response;

namespace Deed.Application.Expenses.Responses;

public sealed record ExpenseResponse(
    int Id,
    int CapitalId,
    decimal Amount,
    DateTimeOffset PaymentDate,
    string? Purpose);
