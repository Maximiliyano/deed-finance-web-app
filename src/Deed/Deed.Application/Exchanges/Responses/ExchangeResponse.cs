namespace Deed.Application.Exchanges.Responses;

public sealed record ExchangeResponse(
    string TargetCurrency,
    string NationalCurrency,
    float Buy,
    float Sale,
    DateTimeOffset? UpdatedAt
);
