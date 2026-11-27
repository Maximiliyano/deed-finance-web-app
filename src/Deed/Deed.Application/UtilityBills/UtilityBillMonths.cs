namespace Deed.Application.UtilityBills;

internal static class UtilityBillMonths
{
    internal static (DateTimeOffset From, DateTimeOffset To) Previous(DateTimeOffset date)
    {
        var utc = date.UtcDateTime;
        var currentStart = new DateTimeOffset(utc.Year, utc.Month, 1, 0, 0, 0, TimeSpan.Zero);

        return (currentStart.AddMonths(-1), currentStart);
    }
}
