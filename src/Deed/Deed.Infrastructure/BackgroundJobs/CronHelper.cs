using Quartz;

namespace Deed.Infrastructure.BackgroundJobs;

internal static class CronHelper
{
    internal static bool HasFiredInWindow(string? cronExpression, DateTimeOffset windowStart, DateTimeOffset windowEnd)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            return false;
        }

        try
        {
            var cron = new CronExpression(cronExpression) { TimeZone = TimeZoneInfo.Utc };
            var nextFire = cron.GetTimeAfter(windowStart.AddSeconds(-1));
            return nextFire.HasValue && nextFire.Value <= windowEnd;
        }
        catch
        {
            return false;
        }
    }
}
