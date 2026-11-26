using Deed.Application.BankConnections.Service;
using Quartz;
using Serilog;

namespace Deed.Infrastructure.BackgroundJobs.BankAccountRefresh;

[DisallowConcurrentExecution]
public sealed class BankAccountRefreshJob(IBankAccountRefreshService refreshService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("Job started: {Name}", nameof(BankAccountRefreshJob));

        var refreshed = await refreshService.RefreshAsync(context.CancellationToken).ConfigureAwait(false);

        Log.Information("Job finished: {Name}, accounts refreshed: {Count}", nameof(BankAccountRefreshJob), refreshed);
    }
}
