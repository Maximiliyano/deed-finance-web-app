using Deed.Application.Abstractions.Settings;
using Microsoft.Extensions.Options;
using Quartz;

namespace Deed.Infrastructure.BackgroundJobs.ExpenseReminder;

public sealed class ExpenseReminderJobSetup(
    IOptions<BackgroundJobsSettings> settings)
    : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        var jobKey = JobKey.Create(nameof(ExpenseReminderJob));

        options
            .AddJob<ExpenseReminderJob>(jobBuilder => jobBuilder
            .WithIdentity(jobKey));

        options.AddTrigger(trigger => trigger
            .ForJob(jobKey)
            .WithCronSchedule(settings.Value.CronReminderCheckSchedule));
    }
}
