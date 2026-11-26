using Deed.Domain.Enums;

namespace Deed.Application.BankConnections.Service;

public sealed record BankTransaction(
    string ExternalId,
    BankTransactionDirection Direction,
    decimal Amount,
    string Currency,
    string? Counterparty,
    string? Purpose,
    DateTimeOffset OccurredAt);
