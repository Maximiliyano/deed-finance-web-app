using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Deed.Application.BankConnections.Statements;

internal static class StatementExternalId
{
    private const string Prefix = "mail:";
    private const int HashLength = 32;

    internal static string Compute(
        string iban,
        DateTimeOffset occurredAt,
        decimal amount,
        string currency,
        string? description,
        decimal? balance)
    {
        var source = string.Join(
            '|',
            iban,
            occurredAt.UtcTicks.ToString(CultureInfo.InvariantCulture),
            amount.ToString("0.00", CultureInfo.InvariantCulture),
            currency.ToUpperInvariant(),
            StatementValues.Collapse(description).ToUpperInvariant(),
            balance?.ToString("0.00", CultureInfo.InvariantCulture) ?? string.Empty);

        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(source)));

        return Prefix + hash[..HashLength];
    }

    internal static string WithOrdinal(string id, int ordinal)
        => ordinal == 0 ? id : $"{id}-{ordinal.ToString(CultureInfo.InvariantCulture)}";
}
