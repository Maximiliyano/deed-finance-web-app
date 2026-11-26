using Deed.Application.Abstractions.Services;
using Deed.Application.Exchanges;
using Deed.Application.Exchanges.Responses;
using Deed.Application.Reminders;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using UserSettingsEntity = Deed.Domain.Entities.UserSettings;

namespace Deed.Tests.Unit.Reminders;

public sealed class ReminderDigestServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Since = Now.AddDays(-1);

    private readonly ICapitalRepository _capitals = Substitute.For<ICapitalRepository>();
    private readonly IExpenseRepository _expenses = Substitute.For<IExpenseRepository>();
    private readonly IIncomeRepository _incomes = Substitute.For<IIncomeRepository>();
    private readonly IDebtRepository _debts = Substitute.For<IDebtRepository>();
    private readonly IBankNotificationRepository _bank = Substitute.For<IBankNotificationRepository>();
    private readonly IExchangeRateProvider _rates = Substitute.For<IExchangeRateProvider>();
    private readonly IEmailService _email = Substitute.For<IEmailService>();
    private readonly ReminderDigestService _sut;

    public ReminderDigestServiceTests()
    {
        _rates.GetLatestAsync(Arg.Any<CancellationToken>()).Returns([new ExchangeResponse("USD", "UAH", 40m, 40m, null)]);
        _capitals.GetAllAsync(Arg.Any<ISpecification<Capital>>(), Arg.Any<CancellationToken>()).Returns(
        [
            new Capital(1) { Name = "Wallet", Balance = 100m, Currency = CurrencyType.USD, IncludeInTotal = true },
            new Capital(2) { Name = "Cash", Balance = 400m, Currency = CurrencyType.UAH, IncludeInTotal = true },
            new Capital(3) { Name = "Hidden", Balance = 999m, Currency = CurrencyType.UAH, IncludeInTotal = false }
        ]);
        _expenses.GetTotalsByCategoryAsync(Arg.Any<string>(), Arg.Any<IReadOnlyCollection<int>?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns([new CategoryAmount(1, "Food", CurrencyType.UAH, 300m)]);
        _incomes.GetTotalsByCategoryAsync(Arg.Any<string>(), Arg.Any<IReadOnlyCollection<int>?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns([new CategoryAmount(2, "Salary", CurrencyType.UAH, 1000m)]);
        _debts.GetAllAsync(Arg.Any<ISpecification<Debt>>(), Arg.Any<CancellationToken>()).Returns(
        [
            new Debt(1) { Item = "<b>Laptop</b>", Amount = 50m, Currency = CurrencyType.USD, Source = "s", Recipient = "r", BorrowedAt = Now.AddDays(-30), DeadlineAt = Now.AddDays(-1) }
        ]);
        _bank.GetAllAsync(Arg.Any<ISpecification<BankNotification>>(), Arg.Any<CancellationToken>()).Returns(
        [
            new BankNotification(1) { ExternalId = "1", Type = BankTransactionType.TopUp, Direction = BankTransactionDirection.Incoming, Amount = 200m, Currency = "UAH", OccurredAt = Now.AddHours(-2) },
            new BankNotification(2) { ExternalId = "2", Type = BankTransactionType.Withdrawal, Direction = BankTransactionDirection.Outgoing, Amount = 50m, Currency = "UAH", OccurredAt = Now.AddHours(-3) }
        ]);

        _sut = new ReminderDigestService(_capitals, _expenses, _incomes, _debts, _bank, _rates, _email);
    }

    [Fact]
    public async Task SendAsync_AllSections_SendsSingleEmailWithEverySection()
    {
        // Arrange
        var settings = CreateSettings(bankAlerts: true);
        var request = new ReminderDigestRequest(true, true, Since);
        string? body = null;
        await _email.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Do<string>(b => body = b), Arg.Any<CancellationToken>());

        // Act
        var sent = await _sut.SendAsync(settings, request, Now);

        // Assert
        sent.Should().BeTrue();
        await _email.Received(1).SendAsync("user@test.com", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        body.Should().Contain("<h3>Balance</h3>").And.Contain("4,400.00 UAH")
            .And.Contain("Expenses &amp; incomes").And.Contain("+1,000.00 UAH").And.Contain("-300.00 UAH")
            .And.Contain("Bank transactions").And.Contain("Top-ups (1)").And.Contain("Withdrawals (1)")
            .And.Contain("<h3>Debts</h3>").And.Contain("OVERDUE");
        body.Should().NotContain("Hidden");
    }

    [Fact]
    public async Task SendAsync_DebtItemWithHtml_EncodesIt()
    {
        // Arrange
        var request = new ReminderDigestRequest(false, true, null);
        string? body = null;
        await _email.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Do<string>(b => body = b), Arg.Any<CancellationToken>());

        // Act
        await _sut.SendAsync(CreateSettings(), request, Now);

        // Assert
        body.Should().Contain("&lt;b&gt;Laptop&lt;/b&gt;").And.NotContain("<b>Laptop</b>");
    }

    [Fact]
    public async Task SendAsync_OnlyBalanceRequested_OmitsOtherSections()
    {
        // Arrange
        var request = new ReminderDigestRequest(true, false, null);
        string? body = null;
        await _email.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Do<string>(b => body = b), Arg.Any<CancellationToken>());

        // Act
        await _sut.SendAsync(CreateSettings(bankAlerts: true), request, Now);

        // Assert
        body.Should().Contain("Balance").And.NotContain("Debts").And.NotContain("Expenses &amp;").And.NotContain("Bank transactions");
        await _debts.DidNotReceive().GetAllAsync(Arg.Any<ISpecification<Debt>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_BankAlertsDisabled_SkipsBankSection()
    {
        // Arrange
        var request = new ReminderDigestRequest(false, false, Since);

        // Act
        await _sut.SendAsync(CreateSettings(bankAlerts: false), request, Now);

        // Assert
        await _bank.DidNotReceive().GetAllAsync(Arg.Any<ISpecification<BankNotification>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_OnlyDebtsRequestedAndNoneExist_DoesNotSend()
    {
        // Arrange
        _debts.GetAllAsync(Arg.Any<ISpecification<Debt>>(), Arg.Any<CancellationToken>()).Returns(Array.Empty<Debt>());

        // Act
        var sent = await _sut.SendAsync(CreateSettings(), new ReminderDigestRequest(false, true, null), Now);

        // Assert
        sent.Should().BeFalse();
        await _email.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_NoExpensesInPeriod_AddsLoggingReminder()
    {
        // Arrange
        _expenses.GetTotalsByCategoryAsync(Arg.Any<string>(), Arg.Any<IReadOnlyCollection<int>?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<CategoryAmount>());
        string? body = null;
        await _email.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Do<string>(b => body = b), Arg.Any<CancellationToken>());

        // Act
        await _sut.SendAsync(CreateSettings(), new ReminderDigestRequest(false, false, Since), Now);

        // Assert
        body.Should().Contain("No expenses were logged");
    }

    [Fact]
    public async Task SendAsync_NoEmail_DoesNotSend()
    {
        // Arrange
        var settings = CreateSettings();
        settings.Email = null;

        // Act
        var sent = await _sut.SendAsync(settings, new ReminderDigestRequest(true, true, Since), Now);

        // Assert
        sent.Should().BeFalse();
    }

    private static UserSettingsEntity CreateSettings(bool bankAlerts = false)
        => new()
        {
            Currency = CurrencyType.UAH,
            Email = "user@test.com",
            EmailNotificationsEnabled = true,
            BankAlertsEmailEnabled = bankAlerts
        };
}
