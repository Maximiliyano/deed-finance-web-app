namespace Deed.Application.Abstractions.Settings;

public sealed class BankSettings
{
    public required string ExchangeRates { get; init; }
    
    public required string PersonalClientInfo { get; init; }
    
    public required string ApiKey { get; init; }
}
