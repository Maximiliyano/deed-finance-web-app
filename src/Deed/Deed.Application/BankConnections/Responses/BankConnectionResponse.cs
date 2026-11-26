namespace Deed.Application.BankConnections.Responses;

public sealed record BankConnectionResponse(
    int Id,
    string Provider,
    string Label,
    string Status,
    string MaskedAccount,
    DateTimeOffset LastSyncAt);
