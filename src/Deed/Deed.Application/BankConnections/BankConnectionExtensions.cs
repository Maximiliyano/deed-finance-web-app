using Deed.Application.BankConnections.Responses;
using Deed.Domain.Entities;

namespace Deed.Application.BankConnections;

internal static class BankConnectionExtensions
{
    internal static BankConnectionResponse ToResponse(this BankConnection? connection)
        => connection is null
            ? new(false, null, null, null)
            : new(true, connection.Status.ToString(), MaskIban(connection.AccountIban), connection.LastSyncAt);

    internal static string NormalizeIban(string iban)
        => string.Concat(iban.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();

    private static string MaskIban(string iban)
        => iban.Length <= 8 ? iban : $"{iban[..4]}{new string('*', iban.Length - 8)}{iban[^4..]}";
}
