using System.Text.RegularExpressions;
using Deed.Domain.Enums;

namespace Deed.Application.BankConnections.Service;

internal static partial class BankTransactionClassifier
{
    internal static BankTransactionType Classify(BankTransactionDirection direction, string? counterparty, string? purpose)
    {
        var text = $"{counterparty} {purpose}";

        if (ExchangePattern().IsMatch(text))
        {
            return BankTransactionType.Exchange;
        }

        if (direction == BankTransactionDirection.Outgoing && WithdrawalPattern().IsMatch(text))
        {
            return BankTransactionType.Withdrawal;
        }

        if (direction == BankTransactionDirection.Incoming && TopUpPattern().IsMatch(text))
        {
            return BankTransactionType.TopUp;
        }

        if (TransferPattern().IsMatch(text))
        {
            return BankTransactionType.Transfer;
        }

        return direction == BankTransactionDirection.Incoming ? BankTransactionType.TopUp : BankTransactionType.Payment;
    }

    [GeneratedRegex(@"обмін\s+валют|обмен\s+валют|конвертаці|купівля\s+валют|продаж\s+валют|currency\s+(exchange|conversion)|\bfx\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex ExchangePattern();

    [GeneratedRegex(@"зняття\s+готівки|видача\s+готівки|банкомат|\batm\b|cash\s*(withdrawal|out)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex WithdrawalPattern();

    [GeneratedRegex(@"поповнення|внесення\s+готівки|зарахування\s+готівки|cash\s*(in|deposit)|top[\s-]?up", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex TopUpPattern();

    [GeneratedRegex(@"переказ|перерахування|перевод|\bp2p\b|transfer|(на|з)\s+карт(ку|у|ки|и)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex TransferPattern();
}
