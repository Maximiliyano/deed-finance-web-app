using System.Globalization;
using System.Text;
using Deed.Application.BankConnections.Service;

namespace Deed.Application.BankConnections.Statements;

internal readonly record struct ParsedAmount(decimal Value, string? Currency);

internal static class StatementValues
{
    private const double MinSerialDate = 20000;
    private const double MaxSerialDate = 80000;

    private static readonly string[] DateFormats =
    [
        "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm", "dd.MM.yyyy",
        "d.M.yyyy H:mm:ss", "d.M.yyyy H:mm", "d.M.yyyy",
        "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm", "dd/MM/yyyy",
        "dd-MM-yyyy HH:mm:ss", "dd-MM-yyyy HH:mm", "dd-MM-yyyy",
        "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-MM-dd",
        "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm",
        "dd.MM.yy HH:mm:ss", "dd.MM.yy HH:mm", "dd.MM.yy"
    ];

    private static readonly string[] OffsetFormats = ["yyyy-MM-ddTHH:mm:sszzz", "yyyy-MM-ddTHH:mm:ss'Z'"];

    private static readonly string[] TimeFormats = ["H:mm:ss", "H:mm", "HH:mm:ss", "HH:mm"];

    private static readonly (string Marker, string Code)[] CurrencyMarkers =
    [
        ("₴", "UAH"), ("грн", "UAH"), ("uah", "UAH"),
        ("$", "USD"), ("usd", "USD"),
        ("€", "EUR"), ("eur", "EUR"),
        ("zł", "PLN"), ("pln", "PLN"),
        ("kč", "CZK"), ("czk", "CZK")
    ];

    internal static string Collapse(string? text)
        => string.Join(' ', (text ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    internal static string NormalizeHeader(string? text)
    {
        var builder = new StringBuilder();

        foreach (var c in (text ?? string.Empty).Trim())
        {
            builder.Append(char.IsLetterOrDigit(c) ? c : ' ');
        }

        return Collapse(builder.ToString());
    }

    internal static ParsedAmount? ParseAmount(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var value = text.Trim();
        var negative = value.Contains('-') || value.Contains('−') || value.Contains('–') || value.StartsWith('(') && value.EndsWith(')');
        var digits = new StringBuilder(value.Length);

        foreach (var c in value)
        {
            if (char.IsAsciiDigit(c) || c is '.' or ',')
            {
                digits.Append(c);
            }
        }

        if (digits.Length == 0 || !decimal.TryParse(NormalizeSeparators(digits.ToString()), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
        {
            return null;
        }

        return new ParsedAmount(negative ? -number : number, DetectCurrency(value));
    }

    internal static DateTimeOffset? ParseDateTime(string? date, string? time)
    {
        var text = Collapse(date);

        if (text.Length == 0)
        {
            return null;
        }

        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var serial))
        {
            return serial is > MinSerialDate and < MaxSerialDate
                ? KyivTime.ToOffset(WithTime(DateTime.FromOADate(serial), time))
                : null;
        }

        if (DateTimeOffset.TryParseExact(text, OffsetFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var withOffset))
        {
            return withOffset;
        }

        return DateTime.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var local)
            ? KyivTime.ToOffset(WithTime(local, time))
            : null;
    }

    internal static decimal? ParseDecimal(string? text)
        => ParseAmount(text)?.Value;

    private static DateTime WithTime(DateTime local, string? time)
    {
        if (local.TimeOfDay != TimeSpan.Zero)
        {
            return local;
        }

        var text = Collapse(time);

        if (DateTime.TryParseExact(text, TimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return local.Date + parsed.TimeOfDay;
        }

        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var fraction) && fraction is > 0 and < 1
            ? local.Date + TimeSpan.FromDays(fraction)
            : local;
    }

    private static string NormalizeSeparators(string digits)
    {
        var lastDot = digits.LastIndexOf('.');
        var lastComma = digits.LastIndexOf(',');

        if (lastDot >= 0 && lastComma >= 0)
        {
            var decimalSeparator = lastDot > lastComma ? '.' : ',';
            var thousands = decimalSeparator == '.' ? ',' : '.';

            return digits.Replace(thousands.ToString(), string.Empty, StringComparison.Ordinal).Replace(decimalSeparator, '.');
        }

        if (lastComma >= 0)
        {
            return ResolveSingleSeparator(digits, ',');
        }

        return lastDot >= 0 ? ResolveSingleSeparator(digits, '.') : digits;
    }

    private static string ResolveSingleSeparator(string digits, char separator)
    {
        var count = digits.Count(c => c == separator);

        if (count > 1)
        {
            return digits.Replace(separator.ToString(), string.Empty, StringComparison.Ordinal);
        }

        var index = digits.IndexOf(separator);
        var after = digits.Length - index - 1;

        if (separator == ',' && after == 3 && index is >= 1 and <= 3)
        {
            return digits.Replace(",", string.Empty, StringComparison.Ordinal);
        }

        return digits.Replace(separator, '.');
    }

    private static string? DetectCurrency(string value)
    {
        foreach (var (marker, code) in CurrencyMarkers)
        {
            if (value.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                return code;
            }
        }

        return null;
    }
}
