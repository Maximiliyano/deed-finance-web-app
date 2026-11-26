using Deed.Application.BankConnections.Responses;
using Deed.Domain.Entities;

namespace Deed.Application.BankConnections;

internal static class BankConnectionExtensions
{
    private const string AllAccounts = "All accounts";

    internal static BankConnectionResponse ToResponse(this BankConnection connection)
        => new(
            connection.Id,
            connection.Provider.ToString(),
            connection.Label,
            connection.Status.ToString(),
            MaskAccount(connection.AccountIban),
            connection.LastSyncAt,
            connection.CapitalId,
            connection.AutoImport,
            connection.ExpenseCategoryId,
            connection.IncomeCategoryId,
            connection.BackfillFrom,
            connection.BackfillCursor);

    internal static IReadOnlyList<BankConnectionResponse> ToResponses(this IEnumerable<BankConnection> connections)
        => [.. connections.Select(c => c.ToResponse())];

    internal static string? NormalizeLabel(string? label)
    {
        var trimmed = label?.Trim();

        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    internal static string NormalizeIban(string? iban)
        => string.Concat((iban ?? string.Empty).Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();

    private static string MaskAccount(string iban)
        => iban.Length switch
        {
            0 => AllAccounts,
            <= 8 => iban,
            _ => $"{iban[..4]}{new string('*', iban.Length - 8)}{iban[^4..]}"
        };
}
