using Deed.Domain.Enums;

namespace Deed.Application.Reminders;

internal sealed record ReminderDigest(
    DateTimeOffset Now,
    CurrencyType Currency,
    DigestBalance? Balance,
    DigestActivity? Activity,
    DigestBank? Bank,
    DigestDebts? Debts)
{
    public bool HasContent => Balance is not null || Activity is not null || Bank is not null || Debts is not null;
}

internal sealed record DigestBalance(decimal Total, IReadOnlyList<DigestCapital> Capitals);

internal sealed record DigestCapital(string Name, decimal Balance, CurrencyType Currency);

internal sealed record DigestActivity(
    DateTimeOffset Since,
    decimal Expenses,
    decimal Incomes,
    IReadOnlyList<DigestCategory> TopExpenses);

internal sealed record DigestCategory(string Name, decimal Amount);

internal sealed record DigestBank(
    int Count,
    IReadOnlyList<DigestBankTotal> Totals,
    IReadOnlyList<DigestBankItem> Latest);

internal sealed record DigestBankTotal(
    BankTransactionType Type,
    BankTransactionDirection Direction,
    string Currency,
    int Count,
    decimal Amount);

internal sealed record DigestBankItem(
    DateTimeOffset OccurredAt,
    BankTransactionType Type,
    BankTransactionDirection Direction,
    decimal Amount,
    string Currency,
    string? Counterparty,
    string? AccountLabel);

internal sealed record DigestDebts(IReadOnlyList<DigestDebt> Items);

internal sealed record DigestDebt(string Item, decimal Amount, CurrencyType Currency, DateTimeOffset? Deadline);
