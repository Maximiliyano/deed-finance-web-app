using Deed.Application.BankConnections.Service;
using Deed.Infrastructure.BackgroundJobs.BankAccountRefresh;
using NSubstitute;
using Quartz;

namespace Deed.Tests.Unit.BackgroundJobs;

public sealed class BankAccountRefreshJobTests
{
    [Fact]
    public async Task Execute_RunsTheServiceOnce()
    {
        // Arrange
        var service = Substitute.For<IBankAccountRefreshService>();
        var context = Substitute.For<IJobExecutionContext>();
        var job = new BankAccountRefreshJob(service);

        // Act
        await job.Execute(context);

        // Assert
        await service.Received(1).RefreshAsync(Arg.Any<CancellationToken>());
    }
}
