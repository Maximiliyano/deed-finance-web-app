namespace Deed.Application.BankConnections.Service;

public sealed record PrivatAccount(
    string Iban,
    string Currency,
    string? Name,
    decimal? Balance);
