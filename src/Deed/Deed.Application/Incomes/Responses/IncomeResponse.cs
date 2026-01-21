namespace Deed.Application.Incomes.Responses;

public sealed record IncomeResponse(
    int Id,
    int CapitalId,
    int CategoryId,
    decimal Amount,
    DateTimeOffset PaymentDate,
    string? Purpose
);
