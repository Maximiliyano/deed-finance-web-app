namespace Deed.Application.Abstractions.Settings;

public sealed class MemoryCacheSettings
{
    public int ExchangesTimespanInHours { get; init; }

    public int CategoriesTimespanInHours { get; init; }
}
