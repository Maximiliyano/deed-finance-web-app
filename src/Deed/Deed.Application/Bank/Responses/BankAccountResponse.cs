namespace Deed.Application.Bank.Responses;

public sealed record BankAccountResponse(
    string Id,
    string SendId,
    double Balance,
    double CreditLimit,
    string Type, // enum
    int CurrencyCode,
    string CashbackType, // type enum
    string[] MaskedPan,
    string Iban
);
