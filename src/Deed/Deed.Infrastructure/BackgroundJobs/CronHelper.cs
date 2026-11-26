using Quartz;

namespace Deed.Infrastructure.BackgroundJobs;

internal static class CronHelper
{
    internal static bool HasFiredInWindow(string? cronExpression, DateTimeOffset windowStart, DateTimeOffset windowEnd)
        => GetPeriodStart(cronExpression, windowStart, windowEnd) is not null;

    internal static DateTimeOffset? GetPeriodStart(string? cronExpression, DateTimeOffset windowStart, DateTimeOffset windowEnd)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            return null;
        }

        try
        {
            var cron = new CronExpression(cronExpression) { TimeZone = TimeZoneInfo.Utc };
            var fire = cron.GetTimeAfter(windowStart.AddSeconds(-1));

            if (!fire.HasValue || fire.Value > windowEnd)
            {
                return null;
            }

            var next = cron.GetTimeAfter(fire.Value);

            return next.HasValue ? fire.Value - (next.Value - fire.Value) : fire.Value.AddDays(-1);
        }
        catch
        {
            return null;
        }
    }
}
