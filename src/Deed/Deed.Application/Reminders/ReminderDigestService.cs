using Deed.Application.Abstractions.Services;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.BankNotifications;
using Deed.Application.BankNotifications.Specifications;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Debts.Specifications;
using Deed.Application.Exchanges;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using UserSettingsEntity = Deed.Domain.Entities.UserSettings;

namespace Deed.Application.Reminders;

public sealed class ReminderDigestService(
    ICapitalRepository capitalRepository,
    IExpenseRepository expenseRepository,
    IIncomeRepository incomeRepository,
    IDebtRepository debtRepository,
    IBankNotificationRepository bankNotificationRepository,
    IBankConnectionRepository bankConnectionRepository,
    IExchangeRateProvider exchangeRateProvider,
    IEmailService emailService)
    : IReminderDigestService
{
    private const CurrencyType DefaultCurrency = CurrencyType.UAH;
    private const int TopExpenseCategories = 3;
    private const int LatestBankItems = 10;
    private const int MaxBankRows = 500;

    public async Task<bool> SendAsync(UserSettingsEntity settings, ReminderDigestRequest request, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(settings.Email))
        {
            return false;
        }

        var currency = settings.Currency == CurrencyType.None ? DefaultCurrency : settings.Currency;
        var converter = new CurrencyConverter(await exchangeRateProvider.GetLatestAsync(cancellationToken).ConfigureAwait(false));

        var balance = request.IncludeBalance
            ? await BuildBalanceAsync(settings.CreatedBy, converter, currency, cancellationToken).ConfigureAwait(false)
            : null;

        var activity = request.ActivitySince is { } since
            ? await BuildActivityAsync(settings.CreatedBy, converter, currency, since, now, cancellationToken).ConfigureAwait(false)
            : null;

        var bank = request.ActivitySince is { } bankSince && settings.BankAlertsEmailEnabled
            ? await BuildBankAsync(settings.CreatedBy, bankSince, cancellationToken).ConfigureAwait(false)
            : null;

        var debts = request.IncludeDebts
            ? await BuildDebtsAsync(settings.CreatedBy, cancellationToken).ConfigureAwait(false)
            : null;

        var digest = new ReminderDigest(now, currency, balance, activity, bank, debts);

        if (!digest.HasContent)
        {
            return false;
        }

        await emailService
            .SendAsync(settings.Email, "Deed — Your financial summary", ReminderDigestRenderer.Render(digest), cancellationToken)
            .ConfigureAwait(false);

        return true;
    }

    private async Task<DigestBalance> BuildBalanceAsync(string createdBy, CurrencyConverter converter, CurrencyType currency, CancellationToken cancellationToken)
    {
        var capitals = (await capitalRepository
            .GetAllAsync(new CapitalsByQueryParamsSpecification(createdBy), cancellationToken)
            .ConfigureAwait(false))
            .Where(c => c.IncludeInTotal)
            .ToList();

        return new DigestBalance(
            capitals.Sum(c => converter.Convert(c.Balance, c.Currency, currency)),
            [.. capitals.Select(c => new DigestCapital(c.Name, c.Balance, c.Currency))]);
    }

    private async Task<DigestActivity> BuildActivityAsync(string createdBy, CurrencyConverter converter, CurrencyType currency, DateTimeOffset since, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var expenses = await expenseRepository
            .GetTotalsByCategoryAsync(createdBy, null, since, now, cancellationToken)
            .ConfigureAwait(false);

        var incomes = await incomeRepository
            .GetTotalsByCategoryAsync(createdBy, null, since, now, cancellationToken)
            .ConfigureAwait(false);

        var topExpenses = expenses
            .GroupBy(e => e.CategoryName)
            .Select(g => new DigestCategory(g.Key, g.Sum(e => converter.Convert(e.Total, e.Currency, currency))))
            .Where(c => c.Amount > 0)
            .OrderByDescending(c => c.Amount)
            .Take(TopExpenseCategories)
            .ToList();

        return new DigestActivity(
            since,
            expenses.Sum(e => converter.Convert(e.Total, e.Currency, currency)),
            incomes.Sum(i => converter.Convert(i.Total, i.Currency, currency)),
            topExpenses);
    }

    private async Task<DigestBank?> BuildBankAsync(string createdBy, DateTimeOffset since, CancellationToken cancellationToken)
    {
        var connections = await bankConnectionRepository
            .GetAllAsync(new BankConnectionByUserSpecification(createdBy), cancellationToken)
            .ConfigureAwait(false);

        var (connectionIds, _) = BankConnectionScope.Resolve(connections, null, null);

        var notifications = (await bankNotificationRepository
            .GetAllAsync(new BankNotificationsByUserSpecification(createdBy, take: MaxBankRows, since: since, kind: BankNotificationKind.Transaction, connectionIds: connectionIds), cancellationToken)
            .ConfigureAwait(false))
            .ToList();

        if (notifications.Count == 0)
        {
            return null;
        }

        var total = await bankNotificationRepository
            .CountAsync(new BankNotificationsByUserSpecification(createdBy, since: since, kind: BankNotificationKind.Transaction, connectionIds: connectionIds), cancellationToken)
            .ConfigureAwait(false);

        var totals = notifications
            .GroupBy(n => (n.Type, n.Direction, Currency: n.Currency ?? string.Empty))
            .Select(g => new DigestBankTotal(g.Key.Type, g.Key.Direction, g.Key.Currency, g.Count(), g.Sum(n => n.Amount)))
            .OrderBy(t => t.Type)
            .ThenBy(t => t.Direction)
            .ToList();

        var latest = notifications
            .Take(LatestBankItems)
            .Select(n => new DigestBankItem(n.OccurredAt, n.Type, n.Direction, n.Amount, n.Currency ?? string.Empty, n.Counterparty, n.AccountLabel))
            .ToList();

        return new DigestBank(Math.Max(total, notifications.Count), totals, latest);
    }

    private async Task<DigestDebts?> BuildDebtsAsync(string createdBy, CancellationToken cancellationToken)
    {
        var debts = (await debtRepository
            .GetAllAsync(new UnpaidDebtsByUserSpecification(createdBy), cancellationToken)
            .ConfigureAwait(false))
            .ToList();

        return debts.Count == 0
            ? null
            : new DigestDebts([.. debts.Select(d => new DigestDebt(d.Item, d.Amount, d.Currency, d.DeadlineAt))]);
    }
}
