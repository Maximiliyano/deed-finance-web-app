namespace Deed.Application.Incomes.Requests;

public sealed record CreateIncomeRequest(
    int CapitalId,
    int CategoryId,
    decimal Amount,
    DateTimeOffset PaymentDate,
    string? Purpose);
