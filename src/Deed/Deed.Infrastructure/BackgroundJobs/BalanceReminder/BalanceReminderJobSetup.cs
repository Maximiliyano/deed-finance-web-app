using Deed.Application.Abstractions.Settings;
using Microsoft.Extensions.Options;
using Quartz;

namespace Deed.Infrastructure.BackgroundJobs.BalanceReminder;

public sealed class BalanceReminderJobSetup(
    IOptions<BackgroundJobsSettings> settings)
    : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        var jobKey = JobKey.Create(nameof(BalanceReminderJob));

        options
            .AddJob<BalanceReminderJob>(jobBuilder => jobBuilder
            .WithIdentity(jobKey));

        options.AddTrigger(trigger => trigger
            .ForJob(jobKey)
            .WithCronSchedule(settings.Value.CronReminderCheckSchedule));
    }
}
