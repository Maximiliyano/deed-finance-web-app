using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public sealed record CapitalSummary(
    Capital Capital,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal TotalTransferIn,
    decimal TotalTransferOut);
