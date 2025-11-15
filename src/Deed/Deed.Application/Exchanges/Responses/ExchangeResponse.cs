namespace Deed.Application.Exchanges.Responses;

public sealed record ExchangeResponse(
    string TargetCurrency,
    string NationalCurrency,
    decimal Buy,
    decimal Sale,
    DateTimeOffset? UpdatedAt
);
