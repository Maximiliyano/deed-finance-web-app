using Deed.Application.BankConnections.Service;
using Deed.Infrastructure.BackgroundJobs.BankBackfill;
using NSubstitute;
using Quartz;

namespace Deed.Tests.Unit.BackgroundJobs;

public sealed class BankBackfillJobTests
{
    [Fact]
    public async Task Execute_RunsTheServiceOnce()
    {
        // Arrange
        var service = Substitute.For<IBankTransactionSyncService>();
        var context = Substitute.For<IJobExecutionContext>();
        var job = new BankBackfillJob(service);

        // Act
        await job.Execute(context);

        // Assert
        await service.Received(1).BackfillAsync(Arg.Any<CancellationToken>());
    }
}
