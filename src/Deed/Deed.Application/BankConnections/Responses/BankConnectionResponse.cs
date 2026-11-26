namespace Deed.Application.BankConnections.Responses;

public sealed record BankConnectionResponse(
    bool Connected,
    string? Status,
    string? MaskedIban,
    DateTimeOffset? LastSyncAt);
