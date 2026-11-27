using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Results;

namespace Deed.Application.Exchanges;

internal static class ExchangeRateProviderExtensions
{
    internal static async Task<Result<decimal>> ConvertAsync(
        this IExchangeRateProvider provider,
        decimal amount,
        CurrencyType from,
        CurrencyType to,
        CancellationToken cancellationToken)
    {
        if (from == to)
        {
            return Result.Success(amount);
        }

        var rates = await provider.GetLatestAsync(cancellationToken).ConfigureAwait(false);

        return new CurrencyConverter(rates).TryConvert(amount, from, to, out var converted)
            ? Result.Success(Math.Round(converted, 2, MidpointRounding.AwayFromZero))
            : Result.Failure<decimal>(DomainErrors.Exchange.RateNotFound);
    }
}
