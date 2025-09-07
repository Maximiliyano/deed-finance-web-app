using System.Globalization;
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

    private const string LogMessage = "Error getting currencies with reason: {Message}";

    public async Task<Result<IEnumerable<Exchange>>> GetCurrencyAsync()
    {
        try
        {
            Log.Information("Start getting currencies");
            using var request = new HttpRequestMessage(HttpMethod.Get, string.Format(CultureInfo.CurrentCulture, options.Value.ExchangeRatesPrivatAPIUrl, $"{dateTimeProvider.UtcNow.Day:D2}.{dateTimeProvider.UtcNow.Month:D2}.{dateTimeProvider.UtcNow.Year}"));

            Log.Information("Sending request to Privat24API");
            using var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning(LogMessage, response.ReasonPhrase);
                return Result.Failure<IEnumerable<Exchange>>(DomainErrors.Exchange.HttpExecution);
            }

            var content = await response.Content.ReadAsStringAsync();

            Log.Information("Deserializing a response from the content: {Content}", content);
            var exchanges = JsonSerializer.Deserialize<ExchangeRateData>(content, CaseInsensitive);

            if (exchanges is null)
            {
                Log.Warning(LogMessage, DomainErrors.Exchange.Serialization);
                return Result.Failure<IEnumerable<Exchange>>(DomainErrors.Exchange.Serialization);
            }

            var newExchanges = exchanges.ExchangeRates
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
