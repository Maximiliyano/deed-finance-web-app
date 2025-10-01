using Deed.Application.Abstractions.Settings;
using Microsoft.Extensions.Options;
using Quartz;

namespace Deed.Infrastructure.BackgroundJobs.SaveLatestExchange;

public sealed class UpsertLatestExchangeJobSetup(
    IOptions<BackgroundJobsSettings> settings)
    : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        var jobKey = JobKey.Create(nameof(UpsertLatestExchangeJob));

        options
            .AddJob<UpsertLatestExchangeJob>(jobBuilder => jobBuilder
            .WithIdentity(jobKey));

        options.AddTrigger(trigger => trigger
            .ForJob(jobKey)
            .WithCronSchedule(settings.Value.CronExchangeSchedule));

        options.AddTrigger(trigger => trigger
            .ForJob(jobKey)
            .StartNow());
    }
}
