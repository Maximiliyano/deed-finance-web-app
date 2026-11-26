using Deed.Application.BankConnections.Service;
using Quartz;
using Serilog;

namespace Deed.Infrastructure.BackgroundJobs.BankBackfill;

[DisallowConcurrentExecution]
public sealed class BankBackfillJob(IBankTransactionSyncService syncService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("Job started: {Name}", nameof(BankBackfillJob));

        await syncService.BackfillAsync(context.CancellationToken).ConfigureAwait(false);

        Log.Information("Job finished: {Name}", nameof(BankBackfillJob));
    }
}
