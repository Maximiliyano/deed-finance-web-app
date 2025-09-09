using Deed.Domain.Enums;

namespace Deed.Application.Capitals.Requests;

public sealed record CreateCapitalRequest(
    string Name,
    float Balance,
    CurrencyType Currency,
    bool IncludeInTotal,
    bool OnlyForSavings
);
