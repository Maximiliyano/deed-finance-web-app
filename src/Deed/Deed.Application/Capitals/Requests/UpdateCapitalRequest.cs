using Deed.Domain.Enums;

namespace Deed.Application.Capitals.Requests;

public sealed record UpdateCapitalRequest(
    string? Name,
    decimal? Balance,
    CurrencyType? Currency,
    bool? IncludeInTotal,
    bool? OnlyForSavings
);
