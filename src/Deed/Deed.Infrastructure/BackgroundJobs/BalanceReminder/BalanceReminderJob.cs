using Deed.Application.Abstractions.Services;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Exchanges.Specifications;
using Deed.Domain.Repositories;
using Quartz;
using Serilog;

namespace Deed.Infrastructure.BackgroundJobs.BalanceReminder;

[DisallowConcurrentExecution]
public sealed class BalanceReminderJob(
    IUserSettingsRepository userSettingsRepository,
    ICapitalRepository capitalRepository,
    IExchangeRepository exchangeRepository,
    IEmailService emailService)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("Job started: {Name}", nameof(BalanceReminderJob));

        var now = DateTimeOffset.UtcNow;
        var windowStart = now.AddMinutes(-15);

        var allSettings = await userSettingsRepository.GetAllWithRemindersEnabledAsync(context.CancellationToken).ConfigureAwait(false);

        foreach (var settings in allSettings)
        {
            if (!settings.BalanceReminderEnabled || !settings.EmailNotificationsEnabled || string.IsNullOrWhiteSpace(settings.Email))
            {
                continue;
            }

            if (!CronHelper.HasFiredInWindow(settings.BalanceReminderCron, windowStart, now))
            {
                continue;
            }

            try
            {
                var capitals = (await capitalRepository
                    .GetAllAsync(new CapitalsByQueryParamsSpecification(settings.CreatedBy, disableIncludes: true), context.CancellationToken)
                    .ConfigureAwait(false)).ToList();

                var exchanges = (await exchangeRepository
                    .GetAllAsync(new ExchangesByQuerySpecification(), context.CancellationToken)
                    .ConfigureAwait(false)).ToList();

                var mainCurrency = settings.Currency.ToString();
                decimal totalBalance = 0;

                foreach (var capital in capitals)
                {
                    if (!capital.IncludeInTotal)
                    {
                        continue;
                    }

                    var balance = capital.Balance;
                    var capitalCurrency = capital.Currency.ToString();

                    if (capitalCurrency == mainCurrency)
                    {
                        totalBalance += balance;
                        continue;
                    }

                    var direct = exchanges.Find(e => e.NationalCurrencyCode == mainCurrency && e.TargetCurrencyCode == capitalCurrency);
                    if (direct is not null)
                    {
                        totalBalance += balance * direct.Sale;
                        continue;
                    }

                    var reverse = exchanges.Find(e => e.NationalCurrencyCode == capitalCurrency && e.TargetCurrencyCode == mainCurrency);
                    if (reverse is not null && reverse.Buy > 0)
                    {
                        totalBalance += balance / reverse.Buy;
                    }
                }

                var html = $"""
                    <h2>Balance Summary</h2>
                    <p>Your total balance across all capitals is <strong>{totalBalance:N2} {mainCurrency}</strong>.</p>
                    <p>You have <strong>{capitals.Count}</strong> capital(s) tracked.</p>
                    <br/>
                    <p style="color:#888;font-size:12px">This is an automated reminder from Deed Finance.</p>
                    """;

                await emailService.SendAsync(settings.Email, "Deed — Balance Summary", html, context.CancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send balance reminder for user {User}", settings.CreatedBy);
            }
        }

        Log.Information("Job finished: {Name}", nameof(BalanceReminderJob));
    }
}
