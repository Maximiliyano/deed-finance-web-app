namespace Deed.Application.BankConnections.Responses;

public sealed record BankConnectionResponse(
    int Id,
    string Provider,
    string Label,
    string Status,
    string MaskedAccount,
    DateTimeOffset LastSyncAt,
    int? CapitalId,
    bool AutoImport,
    int? ExpenseCategoryId,
    int? IncomeCategoryId,
    DateTimeOffset? BackfillFrom,
    DateTimeOffset? BackfillCursor,
    bool IsExcluded,
    string? MailKey = null);
