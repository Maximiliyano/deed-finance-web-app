using Deed.Application.BankConnections.Service;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;

namespace Deed.Infrastructure.BackgroundJobs.BankBackfill;

internal sealed class QuartzBankBackfillTrigger(IServiceProvider serviceProvider) : IBankBackfillTrigger
{
    public async Task TriggerAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (serviceProvider.GetService<ISchedulerFactory>() is not { } factory)
            {
                return;
            }

            var scheduler = await factory.GetScheduler(cancellationToken).ConfigureAwait(false);

            await scheduler.TriggerJob(JobKey.Create(nameof(BankBackfillJob)), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Warning(ex, "Could not start the bank history job right away, it will run on its schedule");
        }
    }
}
