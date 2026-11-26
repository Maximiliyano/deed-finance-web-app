namespace Deed.Application.BankConnections.Service;

internal static class KyivTime
{
    private static readonly TimeZoneInfo Zone = Resolve();

    internal static DateTime ToLocal(DateTimeOffset value)
        => TimeZoneInfo.ConvertTime(value, Zone).DateTime;

    internal static DateTimeOffset ToOffset(DateTime local)
    {
        var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);

        return new DateTimeOffset(unspecified, Zone.GetUtcOffset(unspecified));
    }

    private static TimeZoneInfo Resolve()
    {
        foreach (var id in new[] { "Europe/Kyiv", "Europe/Kiev", "FLE Standard Time" })
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone))
            {
                return zone;
            }
        }

        return TimeZoneInfo.CreateCustomTimeZone("Kyiv", TimeSpan.FromHours(2), "Kyiv", "Kyiv");
    }
}
