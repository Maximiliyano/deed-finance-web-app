namespace Deed.Application.BankConnections.Service;

public sealed record MonobankAccount(
    string Id,
    string Iban,
    string Currency,
    string? MaskedCard,
    string Type,
    decimal Balance);
