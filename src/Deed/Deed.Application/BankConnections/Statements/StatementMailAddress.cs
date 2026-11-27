using System.Security.Cryptography;

namespace Deed.Application.BankConnections.Statements;

internal static class StatementMailAddress
{
    internal const string KeyPlaceholder = "{key}";

    private const int KeyBytes = 8;
    private const int KeyLength = KeyBytes * 2;

    internal static string NewKey()
        => RandomNumberGenerator.GetHexString(KeyLength);

    internal static string? Template(string address)
    {
        var at = address.LastIndexOf('@');

        return at <= 0 || at == address.Length - 1
            ? null
            : $"{address[..at].Split('+')[0]}+{KeyPlaceholder}{address[at..]}";
    }

    internal static IReadOnlyList<string> ExtractKeys(IEnumerable<string?> addresses, string baseAddress)
    {
        var at = baseAddress.LastIndexOf('@');

        if (at <= 0)
        {
            return [];
        }

        var prefix = baseAddress[..at].Split('+')[0].ToUpperInvariant() + "+";
        var domain = baseAddress[(at + 1)..].ToUpperInvariant();
        var keys = new List<string>();

        foreach (var address in addresses)
        {
            var value = address?.Trim().Trim('<', '>').ToUpperInvariant();
            var separator = value?.LastIndexOf('@') ?? -1;

            if (value is null || separator <= 0 || value[(separator + 1)..] != domain)
            {
                continue;
            }

            var local = value[..separator];

            if (local.StartsWith(prefix, StringComparison.Ordinal) && IsKey(local[prefix.Length..]) && !keys.Contains(local[prefix.Length..]))
            {
                keys.Add(local[prefix.Length..]);
            }
        }

        return keys;
    }

    private static bool IsKey(string value)
        => value.Length == KeyLength && value.All(char.IsAsciiHexDigitUpper);
}
