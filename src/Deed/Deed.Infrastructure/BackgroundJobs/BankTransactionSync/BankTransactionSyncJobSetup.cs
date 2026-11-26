using Deed.Application.Abstractions.Settings;
using Microsoft.Extensions.Options;
using Quartz;

namespace Deed.Infrastructure.BackgroundJobs.BankTransactionSync;

public sealed class BankTransactionSyncJobSetup(
    IOptions<BackgroundJobsSettings> settings)
    : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        var jobKey = JobKey.Create(nameof(BankTransactionSyncJob));

        options
            .AddJob<BankTransactionSyncJob>(jobBuilder => jobBuilder
            .WithIdentity(jobKey));

        options.AddTrigger(trigger => trigger
            .ForJob(jobKey)
            .WithCronSchedule(settings.Value.CronBankSyncSchedule));
    }
}
