namespace Deed.Domain.Repositories;

public sealed record BankBackfillState(DateTimeOffset? Cursor, bool IsExcluded);
