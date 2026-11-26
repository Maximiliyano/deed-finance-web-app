namespace Deed.Application.Abstractions.Settings;

public sealed class BackgroundJobsSettings
{
    public required string CronExchangeSchedule { get; init; }

    public string CronReminderCheckSchedule { get; init; } = "0 0/15 * * * ?";

    public string CronBankSyncSchedule { get; init; } = "0 0/5 * * * ?";

    public string CronBankBackfillSchedule { get; init; } = "0 * * * * ?";

    public string CronBankAccountRefreshSchedule { get; init; } = "0 0 5 * * ?";
}
