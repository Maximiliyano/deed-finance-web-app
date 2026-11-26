using Deed.Application.BankConnections.Service;
using Quartz;
using Serilog;

namespace Deed.Infrastructure.BackgroundJobs.BankTransactionSync;

[DisallowConcurrentExecution]
public sealed class BankTransactionSyncJob(IBankTransactionSyncService syncService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("Job started: {Name}", nameof(BankTransactionSyncJob));

        await syncService.SyncAsync(context.CancellationToken).ConfigureAwait(false);

        Log.Information("Job finished: {Name}", nameof(BankTransactionSyncJob));
    }
}
