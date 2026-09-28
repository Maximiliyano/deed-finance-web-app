namespace Deed.Application.Bank.Responses;

public sealed record BankJar(
    string Id,
    string SendId,
    string Title,
    string Description,
    int CurrencyCode,
    double Balance,
    double? Goal
);
