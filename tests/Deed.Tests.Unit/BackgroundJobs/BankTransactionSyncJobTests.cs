using Deed.Application.BankConnections.Service;
using Deed.Infrastructure.BackgroundJobs.BankTransactionSync;
using NSubstitute;
using Quartz;

namespace Deed.Tests.Unit.BackgroundJobs;

public sealed class BankTransactionSyncJobTests
{
    [Fact]
    public async Task Execute_RunsTheServiceOnce()
    {
        // Arrange
        var service = Substitute.For<IBankTransactionSyncService>();
        var context = Substitute.For<IJobExecutionContext>();
        var job = new BankTransactionSyncJob(service);

        // Act
        await job.Execute(context);

        // Assert
        await service.Received(1).SyncAsync(Arg.Any<CancellationToken>());
    }
}
