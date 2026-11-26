using Deed.Domain.Enums;

namespace Deed.Domain.Repositories;

public sealed record BankCounterpartyTotal(
    string? Currency,
    BankTransactionDirection Direction,
    string? Counterparty,
    decimal Amount,
    int Count);
