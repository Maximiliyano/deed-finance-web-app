namespace Deed.Application.BankNotifications.Responses;

public sealed record BankTransactionsSummaryResponse(
    string Period,
    DateTimeOffset From,
    DateTimeOffset To,
    IReadOnlyList<BankSummaryCurrencyResponse> Currencies);

public sealed record BankSummaryCurrencyResponse(
    string Currency,
    decimal Income,
    decimal Expense,
    decimal ExchangeIn,
    decimal ExchangeOut,
    int Count,
    IReadOnlyList<BankSummaryPeriodResponse> Periods,
    IReadOnlyList<BankSummaryTypeResponse> Types,
    IReadOnlyList<BankSummaryAccountResponse> Accounts,
    IReadOnlyList<BankSummaryCounterpartyResponse> TopIncome,
    IReadOnlyList<BankSummaryCounterpartyResponse> TopExpense);

public sealed record BankSummaryPeriodResponse(DateOnly Start, decimal Income, decimal Expense, int Count);

public sealed record BankSummaryTypeResponse(string Direction, string Type, decimal Amount, int Count);

public sealed record BankSummaryAccountResponse(int? ConnectionId, string Label, decimal Income, decimal Expense, int Count);

public sealed record BankSummaryCounterpartyResponse(string? Name, decimal Amount, int Count);
