namespace Deed.Application.Abstractions.Settings;

public sealed class WebUrlSettings
{
    public required string DeedApiUrl { get; init; }

    public required string UIUrl { get; init; }

    public required string ExchangeRatesPrivatAPIUrl { get; init; }
}
