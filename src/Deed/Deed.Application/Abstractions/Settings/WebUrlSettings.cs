namespace Deed.Application.Abstractions.Settings;

public sealed class WebUrlSettings
{
    public required string UIUrl { get; init; }

    public required string ExchangeRatesPrivatAPIUrl { get; init; }
    
    public required string[] AllowedHeaders { get; init; }
    
    public required string[] AllowedMethods { get; init; }
}
