using Deed.Application.Abstractions.Services;
using Deed.Application.Debts.Specifications;
using Deed.Domain.Repositories;
using Quartz;
using Serilog;

namespace Deed.Infrastructure.BackgroundJobs.DebtReminder;

[DisallowConcurrentExecution]
public sealed class DebtReminderJob(
    IUserSettingsRepository userSettingsRepository,
    IDebtRepository debtRepository,
    IEmailService emailService)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("Job started: {Name}", nameof(DebtReminderJob));

        var now = DateTimeOffset.UtcNow;
        var windowStart = now.AddMinutes(-15);

        var allSettings = await userSettingsRepository.GetAllWithRemindersEnabledAsync(context.CancellationToken).ConfigureAwait(false);

        foreach (var settings in allSettings)
        {
            if (!settings.DebtReminderEnabled || !settings.EmailNotificationsEnabled || string.IsNullOrWhiteSpace(settings.Email))
            {
                continue;
            }

            if (!CronHelper.HasFiredInWindow(settings.DebtReminderCron, windowStart, now))
            {
                continue;
            }

            try
            {
                var debts = (await debtRepository
                    .GetAllAsync(new DebtsByUserSpecification(settings.CreatedBy), context.CancellationToken)
                    .ConfigureAwait(false))
                    .Where(d => !d.IsPaid)
                    .ToList();

                if (debts.Count == 0)
                {
                    continue;
                }

                var overdue = debts.Where(d => d.DeadlineAt.HasValue && d.DeadlineAt.Value < now).ToList();
                var dueSoon = debts.Where(d => d.DeadlineAt.HasValue && d.DeadlineAt.Value >= now && d.DeadlineAt.Value <= now.AddDays(7)).ToList();

                var debtLines = string.Join("", debts.Select(d =>
                {
                    var status = GetDebtStatusLabel(d.DeadlineAt, now);
                    var deadline = d.DeadlineAt.HasValue ? $" (due {d.DeadlineAt.Value:dd MMM yyyy})" : "";
                    return $"<li>{d.Item}: {d.Amount:N2} {d.Currency}{deadline}{status}</li>";
                }));

                var html = $"""
                    <h2>Debt Reminder</h2>
                    <p>You have <strong>{debts.Count}</strong> unpaid debt(s):</p>
                    <ul>{debtLines}</ul>
                    {(overdue.Count > 0 ? $"<p style=\"color:red\"><strong>{overdue.Count} debt(s) are overdue!</strong></p>" : "")}
                    {(dueSoon.Count > 0 ? $"<p style=\"color:orange\"><strong>{dueSoon.Count} debt(s) are due within 7 days.</strong></p>" : "")}
                    <br/>
                    <p style="color:#888;font-size:12px">This is an automated reminder from Deed Finance.</p>
                    """;

                await emailService.SendAsync(settings.Email, "Deed — Debt Reminder", html, context.CancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send debt reminder for user {User}", settings.CreatedBy);
            }
        }

        Log.Information("Job finished: {Name}", nameof(DebtReminderJob));
    }

    private static string GetDebtStatusLabel(DateTimeOffset? deadlineAt, DateTimeOffset now)
    {
        if (!deadlineAt.HasValue)
        {
            return "";
        }

        if (deadlineAt.Value < now)
        {
            return " — <strong style=\"color:red\">OVERDUE</strong>";
        }

        if (deadlineAt.Value <= now.AddDays(7))
        {
            return " — <strong style=\"color:orange\">Due soon</strong>";
        }

        return "";
    }
}
