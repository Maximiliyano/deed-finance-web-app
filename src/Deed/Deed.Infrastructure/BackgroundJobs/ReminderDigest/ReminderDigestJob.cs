using Deed.Application.Reminders;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Quartz;
using Serilog;

namespace Deed.Infrastructure.BackgroundJobs.ReminderDigest;

[DisallowConcurrentExecution]
public sealed class ReminderDigestJob(
    IUserSettingsRepository userSettingsRepository,
    IReminderDigestService digestService)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("Job started: {Name}", nameof(ReminderDigestJob));

        var now = DateTimeOffset.UtcNow;
        var windowStart = now.AddMinutes(-15);

        var allSettings = await userSettingsRepository.GetAllWithRemindersEnabledAsync(context.CancellationToken).ConfigureAwait(false);

        foreach (var settings in allSettings)
        {
            if (!settings.EmailNotificationsEnabled || string.IsNullOrWhiteSpace(settings.Email))
            {
                continue;
            }

            var request = BuildRequest(settings, windowStart, now);

            if (request is null)
            {
                continue;
            }

            try
            {
                await digestService.SendAsync(settings, request, now, context.CancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(ex, "Failed to send reminder digest for user {User}", settings.CreatedBy);
            }
        }

        Log.Information("Job finished: {Name}", nameof(ReminderDigestJob));
    }

    private static ReminderDigestRequest? BuildRequest(UserSettings settings, DateTimeOffset windowStart, DateTimeOffset now)
    {
        var includeBalance = settings.BalanceReminderEnabled
            && CronHelper.HasFiredInWindow(settings.BalanceReminderCron, windowStart, now);

        var includeDebts = settings.DebtReminderEnabled
            && CronHelper.HasFiredInWindow(settings.DebtReminderCron, windowStart, now);

        var activitySince = settings.ExpenseReminderEnabled
            ? CronHelper.GetPeriodStart(settings.ExpenseReminderCron, windowStart, now)
            : null;

        return includeBalance || includeDebts || activitySince is not null
            ? new ReminderDigestRequest(includeBalance, includeDebts, activitySince)
            : null;
    }
}
