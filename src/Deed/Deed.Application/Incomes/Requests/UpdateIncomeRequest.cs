namespace Deed.Application.Incomes.Requests;

public sealed record UpdateIncomeRequest(
    int Id,
    int? CategoryId,
    decimal? Amount,
    string? Purpose,
    DateTimeOffset? PaymentDate);
