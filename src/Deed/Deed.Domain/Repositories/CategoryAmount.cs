using Deed.Domain.Enums;

namespace Deed.Domain.Repositories;

public sealed record CategoryAmount(int CategoryId, string CategoryName, CurrencyType Currency, decimal Total);
