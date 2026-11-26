using System.Text.Json.Serialization;

namespace Deed.Application.Exchanges.Responses;

public class ExchangeRateData
{
    [JsonPropertyName("exchangeRate")]
    public IEnumerable<ExchangeRate> ExchangeRates { get; set; }
}
