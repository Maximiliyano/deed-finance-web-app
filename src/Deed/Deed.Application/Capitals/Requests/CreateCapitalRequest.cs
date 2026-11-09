namespace Deed.Application.Capitals.Requests;

public sealed record CreateCapitalRequest(
    string Name,
    decimal Balance,
    string Currency,
    bool IncludeInTotal,
    bool OnlyForSavings
);
