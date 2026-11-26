using Deed.Domain.Enums;

namespace Deed.Application.BankNotifications;

internal static class BankTransactionTypeExtensions
{
    internal static string ToLabel(this BankTransactionType type)
        => type switch
        {
            BankTransactionType.TopUp => "Top-up",
            BankTransactionType.Withdrawal => "Withdrawal",
            BankTransactionType.Transfer => "Transfer",
            BankTransactionType.Exchange => "Exchange",
            _ => "Payment"
        };
}
