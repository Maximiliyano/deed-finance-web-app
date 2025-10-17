using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Exchanges.Responses;
using Deed.Domain.Entities;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace Deed.Application.Exchanges.Service;

public sealed class ExchangeHttpService(
    IDateTimeProvider dateTimeProvider,
    IOptions<WebUrlSettings> options,
    HttpClient client)
    : IExchangeHttpService
{
    private static readonly JsonSerializerOptions CaseInsensitive = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string LogMessage = "Exception occured while execution.";

    private readonly HashSet<string> AllowedCurrencies = [
        "USD",
        "EUR",
        "PLN"
    ];

    public async Task<Result<IEnumerable<Exchange>>> GetCurrenciesAsync()
    {
        try
        {
            Log.Information("Sending request to get currencies...");
            
            var date = dateTimeProvider.UtcNow.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            var url = string.Format(CultureInfo.CurrentCulture, options.Value.ExchangeRatesPrivatAPIUrl, date);
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning(LogMessage, response.ReasonPhrase);
                return Result.Failure<IEnumerable<Exchange>>(DomainErrors.Exchange.HttpExecution);
            }

            Log.Information("Deserializing a response...");
            var exchanges = await response.Content.ReadFromJsonAsync<ExchangeRateData>(CaseInsensitive);

            if (exchanges is null)
            {
                Log.Warning(LogMessage, DomainErrors.Exchange.Serialization);
                return Result.Failure<IEnumerable<Exchange>>(DomainErrors.Exchange.Serialization);
            }
            var newExchanges = exchanges.ExchangeRates
                .Where(e => AllowedCurrencies.Contains(e.Currency))
                .Select(x => new Exchange
                {
                    NationalCurrencyCode = x.BaseCurrency,
                    TargetCurrencyCode = x.Currency,
                    Buy = x.PurchaseRate.HasValue ? (float)x.PurchaseRate.Value : (float)x.PurchaseRateNB,
                    Sale = x.SaleRate.HasValue ? (float)x.SaleRate : (float)x.SaleRateNB,
                    CreatedAt = dateTimeProvider.UtcNow
                });

            Log.Information("Currencies successfully retrieved.");
            return Result.Success(newExchanges);
        }
        catch (Exception e)
        {
            Log.Warning(e, LogMessage);
            return Result.Failure<IEnumerable<Exchange>>(DomainErrors.Exchange.HttpExecution);
        }
    }
}
