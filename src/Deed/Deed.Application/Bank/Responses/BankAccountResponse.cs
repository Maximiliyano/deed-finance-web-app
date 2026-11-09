namespace Deed.Application.Bank.Responses;

public sealed record BankAccountResponse(
    string Id,
    string SendId,
    double Balance,
    double CreditLimit,
    string Type,
    int CurrencyCode,
    string CashbackType,
    string[] MaskedPan,
    string Iban
);
