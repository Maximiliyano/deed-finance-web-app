using Deed.Application.Exchanges.Responses;
using Deed.Domain.Enums;

namespace Deed.Application.Exchanges;

internal sealed class CurrencyConverter(IReadOnlyCollection<ExchangeResponse> rates)
{
    private readonly string? _baseCurrency = rates.FirstOrDefault()?.NationalCurrency;

    public decimal Convert(decimal amount, CurrencyType from, CurrencyType to)
    {
        if (from == to || from == CurrencyType.None || to == CurrencyType.None || rates.Count == 0)
        {
            return amount;
        }

        var fromCode = from.ToString();
        var toCode = to.ToString();

        var direct = Find(toCode, fromCode);
        if (direct is not null)
        {
            return amount * direct.Sale;
        }

        var reverse = Find(fromCode, toCode);
        if (reverse is { Buy: > 0 })
        {
            return amount / reverse.Buy;
        }

        if (_baseCurrency is not null && fromCode != _baseCurrency && toCode != _baseCurrency)
        {
            var fromBase = Find(_baseCurrency, fromCode);
            var toBase = Find(_baseCurrency, toCode);

            if (fromBase is not null && toBase is { Buy: > 0 })
            {
                return amount * fromBase.Sale / toBase.Buy;
            }
        }

        return amount;
    }

    private ExchangeResponse? Find(string nationalCurrency, string targetCurrency) =>
        rates.FirstOrDefault(r => r.NationalCurrency == nationalCurrency && r.TargetCurrency == targetCurrency);
}
