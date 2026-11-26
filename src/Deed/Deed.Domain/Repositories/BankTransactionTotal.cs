using Deed.Domain.Enums;

namespace Deed.Domain.Repositories;

public sealed record BankTransactionTotal(
    int? BankConnectionId,
    string? Currency,
    BankTransactionDirection Direction,
    BankTransactionType Type,
    int Year,
    int Month,
    int Day,
    decimal Amount,
    int Count);
