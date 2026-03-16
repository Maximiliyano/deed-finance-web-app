using Deed.Application.Abstractions.Services;
using Deed.Domain.Repositories;
using Quartz;
using Serilog;

namespace Deed.Infrastructure.BackgroundJobs.ExpenseReminder;

[DisallowConcurrentExecution]
public sealed class ExpenseReminderJob(
    IUserSettingsRepository userSettingsRepository,
    IEmailService emailService)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("Job started: {Name}", nameof(ExpenseReminderJob));

        var now = DateTimeOffset.UtcNow;
        var windowStart = now.AddMinutes(-15);

        var allSettings = await userSettingsRepository.GetAllWithRemindersEnabledAsync(context.CancellationToken).ConfigureAwait(false);

        foreach (var settings in allSettings)
        {
            if (!settings.ExpenseReminderEnabled || !settings.EmailNotificationsEnabled || string.IsNullOrWhiteSpace(settings.Email))
            {
                continue;
            }

            if (!CronHelper.HasFiredInWindow(settings.ExpenseReminderCron, windowStart, now))
            {
                continue;
            }

            try
            {
                const string html = """
                                    <h2>Expense Reminder</h2>
                                    <p>Don't forget to log your expenses today!</p>
                                    <p>Keeping your expenses up to date helps you stay on top of your budget.</p>
                                    <br/>
                                    <p style="color:#888;font-size:12px">This is an automated reminder from Deed Finance.</p>
                                    """;

                await emailService.SendAsync(settings.Email, "Deed — Log Your Expenses", html, context.CancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send expense reminder for user {User}", settings.CreatedBy);
            }
        }

        Log.Information("Job finished: {Name}", nameof(ExpenseReminderJob));
    }
}
