using Deed.Domain.Enums;

namespace Deed.Application.Capitals.Requests;

public sealed record AddCapitalRequest(
    string Name,
    float Balance,
    string Currency,
    bool IncludeInTotal);
