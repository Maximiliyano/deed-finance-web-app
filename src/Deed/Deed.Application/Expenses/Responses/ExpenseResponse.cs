using Deed.Application.Categories.Response;

namespace Deed.Application.Expenses.Responses;

public sealed record ExpenseResponse(
    int Id,
    int CapitalId,
    CategoryResponse? Category,
    float Amount,
    DateTimeOffset PaymentDate,
    string? Purpose);
