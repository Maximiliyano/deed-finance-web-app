namespace Deed.Application.Abstractions.Settings;

public sealed class BackgroundJobsSettings
{
    public required string CronExchangeSchedule { get; init; }

    public string CronReminderCheckSchedule { get; init; } = "0 0/15 * * * ?";
}
