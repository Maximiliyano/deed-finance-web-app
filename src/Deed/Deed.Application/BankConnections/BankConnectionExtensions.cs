using Deed.Application.BankConnections.Responses;
using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Application.BankConnections;

internal static class BankConnectionExtensions
{
    internal static BankConnectionResponse ToResponse(this BankConnection? connection)
        => connection is null
            ? new(false, null, null, null, null)
            : new(true, connection.Provider.ToString(), connection.Status.ToString(), Mask(connection), connection.LastSyncAt);

    internal static string NormalizeIban(string iban)
        => string.Concat(iban.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();

    internal static string NormalizeCardNumber(string card)
        => string.Concat(card.Where(char.IsAsciiDigit));

    internal static bool IsValidCardNumber(string card)
    {
        if (card.Length != 16)
        {
            return false;
        }

        var sum = 0;

        for (var i = 0; i < card.Length; i++)
        {
            var digit = card[card.Length - 1 - i] - '0';

            if (i % 2 == 1)
            {
                digit *= 2;
                digit -= digit > 9 ? 9 : 0;
            }

            sum += digit;
        }

        return sum % 10 == 0;
    }

    private static string Mask(BankConnection connection)
        => connection.Provider == BankProvider.PrivatCard
            ? $"{connection.AccountIban[..4]} **** **** {connection.AccountIban[^4..]}"
            : MaskIban(connection.AccountIban);

    private static string MaskIban(string iban)
        => iban.Length <= 8 ? iban : $"{iban[..4]}{new string('*', iban.Length - 8)}{iban[^4..]}";
}
