namespace Deed.Application.BankConnections.Responses;

public sealed record BankConnectionResponse(
    bool Connected,
    string? Provider,
    string? Status,
    string? MaskedAccount,
    DateTimeOffset? LastSyncAt);
