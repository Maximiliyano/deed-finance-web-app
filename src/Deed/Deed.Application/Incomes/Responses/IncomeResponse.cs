namespace Deed.Application.Incomes.Responses;

public sealed record IncomeResponse(
    int Id,
    int CategoryId,
    decimal Amount,
    string? Purpose,
    DateTimeOffset CreatedAt,
    int? CapitalId);
