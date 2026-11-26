namespace Deed.Application.BankConnections.Responses;

public sealed record BankAccountResponse(
    int Id,
    string Provider,
    string Iban,
    string MaskedAccount,
    string Currency,
    string? Name,
    decimal? Balance,
    DateTimeOffset LastRefreshedAt,
    int? ConnectionId);
