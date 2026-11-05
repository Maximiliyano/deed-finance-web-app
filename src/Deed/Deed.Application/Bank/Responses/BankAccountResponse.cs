namespace Deed.Application.Bank.Responses;

public sealed record BankAccountResponse(
    string Id,
    string SendId,
    double Balance,
    double CreditLimit,
    string Type, // TODO enum
    int CurrencyCode,
    string CashbackType, // TODO type enum
    string[] MaskedPan,
    string Iban
);
