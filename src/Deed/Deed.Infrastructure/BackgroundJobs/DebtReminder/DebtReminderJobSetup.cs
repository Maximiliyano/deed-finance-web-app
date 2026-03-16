using Deed.Application.Abstractions.Settings;
using Microsoft.Extensions.Options;
using Quartz;

namespace Deed.Infrastructure.BackgroundJobs.DebtReminder;

public sealed class DebtReminderJobSetup(
    IOptions<BackgroundJobsSettings> settings)
    : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        var jobKey = JobKey.Create(nameof(DebtReminderJob));

        options
            .AddJob<DebtReminderJob>(jobBuilder => jobBuilder
            .WithIdentity(jobKey));

        options.AddTrigger(trigger => trigger
            .ForJob(jobKey)
            .WithCronSchedule(settings.Value.CronReminderCheckSchedule));
    }
}
