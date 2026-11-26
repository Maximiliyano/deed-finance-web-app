using Deed.Application.Abstractions.Settings;
using Microsoft.Extensions.Options;
using Quartz;

namespace Deed.Infrastructure.BackgroundJobs.BankAccountRefresh;

public sealed class BankAccountRefreshJobSetup(
    IOptions<BackgroundJobsSettings> settings)
    : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        var jobKey = JobKey.Create(nameof(BankAccountRefreshJob));

        options
            .AddJob<BankAccountRefreshJob>(jobBuilder => jobBuilder
            .WithIdentity(jobKey));

        options.AddTrigger(trigger => trigger
            .ForJob(jobKey)
            .WithCronSchedule(settings.Value.CronBankAccountRefreshSchedule));

        options.AddTrigger(trigger => trigger
            .ForJob(jobKey)
            .StartNow());
    }
}
