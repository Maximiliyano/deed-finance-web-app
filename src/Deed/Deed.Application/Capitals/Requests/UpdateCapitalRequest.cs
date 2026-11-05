using Deed.Domain.Enums;

namespace Deed.Application.Capitals.Requests;

public sealed record UpdateCapitalRequest(
    string? Name,
    decimal? Balance,
    string? Currency,
    bool? IncludeInTotal,
    bool? OnlyForSavings
);
