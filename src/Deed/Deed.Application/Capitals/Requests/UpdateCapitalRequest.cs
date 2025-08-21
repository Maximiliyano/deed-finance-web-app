using Deed.Domain.Enums;

namespace Deed.Application.Capitals.Requests;

public sealed record UpdateCapitalRequest(
    string? Name,
    float? Balance,
    CurrencyType? Currency);
