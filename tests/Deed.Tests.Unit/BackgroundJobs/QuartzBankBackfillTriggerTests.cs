using Deed.Infrastructure.BackgroundJobs.BankBackfill;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Quartz;

namespace Deed.Tests.Unit.BackgroundJobs;

public sealed class QuartzBankBackfillTriggerTests
{
    [Fact]
    public async Task TriggerAsync_SchedulerAvailable_FiresTheBackfillJob()
    {
        // Arrange
        var scheduler = Substitute.For<IScheduler>();
        var factory = Substitute.For<ISchedulerFactory>();
        factory.GetScheduler(Arg.Any<CancellationToken>()).Returns(scheduler);
        var trigger = new QuartzBankBackfillTrigger(new ServiceCollection().AddSingleton(factory).BuildServiceProvider());

        // Act
        await trigger.TriggerAsync(CancellationToken.None);

        // Assert
        await scheduler.Received(1).TriggerJob(Arg.Is<JobKey>(key => key.Name == nameof(BankBackfillJob)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TriggerAsync_NoScheduler_DoesNothing()
    {
        // Arrange
        var trigger = new QuartzBankBackfillTrigger(new ServiceCollection().BuildServiceProvider());

        // Act
        var act = () => trigger.TriggerAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task TriggerAsync_SchedulerFails_IsSwallowed()
    {
        // Arrange
        var scheduler = Substitute.For<IScheduler>();
        scheduler.TriggerJob(Arg.Any<JobKey>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(new InvalidOperationException("down")));
        var factory = Substitute.For<ISchedulerFactory>();
        factory.GetScheduler(Arg.Any<CancellationToken>()).Returns(scheduler);
        var trigger = new QuartzBankBackfillTrigger(new ServiceCollection().AddSingleton(factory).BuildServiceProvider());

        // Act
        var act = () => trigger.TriggerAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
