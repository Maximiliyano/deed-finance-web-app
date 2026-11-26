using System.Globalization;
using System.Net;
using Deed.Application.BankNotifications;
using Deed.Domain.Enums;

namespace Deed.Application.Reminders;

internal static class ReminderDigestRenderer
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    internal static string Render(ReminderDigest digest)
    {
        var sections = string.Concat(
            RenderBalance(digest),
            RenderActivity(digest),
            RenderBank(digest),
            RenderDebts(digest));

        return $"""
            <h2>Your Deed Finance summary</h2>
            {sections}
            <br/>
            <p style="color:#888;font-size:12px">This is an automated reminder from Deed Finance. Choose what it includes in your profile notification settings.</p>
            """;
    }

    private static string RenderBalance(ReminderDigest digest)
    {
        if (digest.Balance is not { } balance)
        {
            return string.Empty;
        }

        var capitals = string.Concat(balance.Capitals.Select(c =>
            $"<li>{Encode(c.Name)}: {Money(c.Balance, c.Currency.ToString())}</li>"));

        return $"""
            <h3>Balance</h3>
            <p>Your total balance is <strong>{Money(balance.Total, digest.Currency.ToString())}</strong> across {balance.Capitals.Count.ToString(Culture)} capital(s).</p>
            {(balance.Capitals.Count > 0 ? $"<ul>{capitals}</ul>" : string.Empty)}
            """;
    }

    private static string RenderActivity(ReminderDigest digest)
    {
        if (digest.Activity is not { } activity)
        {
            return string.Empty;
        }

        var currency = digest.Currency.ToString();
        var net = activity.Incomes - activity.Expenses;
        var netColor = net >= 0 ? "green" : "red";
        var categories = string.Concat(activity.TopExpenses.Select(c => $"<li>{Encode(c.Name)}: {Money(c.Amount, currency)}</li>"));
        var period = $"{activity.Since.ToString("dd MMM", Culture)} – {digest.Now.ToString("dd MMM yyyy", Culture)}";

        return $"""
            <h3>Expenses &amp; incomes</h3>
            <p style="color:#888;margin:0">{period}</p>
            <p>
            Incomes: <strong style="color:green">+{Money(activity.Incomes, currency)}</strong><br/>
            Expenses: <strong style="color:red">-{Money(activity.Expenses, currency)}</strong><br/>
            Net: <strong style="color:{netColor}">{(net >= 0 ? "+" : "-")}{Money(Math.Abs(net), currency)}</strong>
            </p>
            {(activity.TopExpenses.Count > 0 ? $"<p>Biggest expense categories:</p><ul>{categories}</ul>" : string.Empty)}
            {(activity.Expenses == 0 ? "<p>No expenses were logged in this period. Don't forget to add them to keep your budget accurate.</p>" : string.Empty)}
            """;
    }

    private static string RenderBank(ReminderDigest digest)
    {
        if (digest.Bank is not { } bank)
        {
            return string.Empty;
        }

        var totals = string.Concat(bank.Totals.Select(t =>
            $"<li>{TotalLabel(t)} ({t.Count.ToString(Culture)}): <strong>{Signed(t.Direction, t.Amount, t.Currency)}</strong></li>"));

        var latest = string.Concat(bank.Latest.Select(i =>
            $"<li>{i.OccurredAt.ToString("dd MMM HH:mm", Culture)} · {i.Type.ToLabel()} <strong>{Signed(i.Direction, i.Amount, i.Currency)}</strong>"
            + $"{(string.IsNullOrWhiteSpace(i.Counterparty) ? string.Empty : $" {Encode(i.Counterparty)}")}"
            + $"{(string.IsNullOrWhiteSpace(i.AccountLabel) ? string.Empty : $" <span style=\"color:#888\">({Encode(i.AccountLabel)})</span>")}</li>"));

        return $"""
            <h3>Bank transactions</h3>
            <p><strong>{bank.Count.ToString(Culture)}</strong> transaction(s) in this period.</p>
            <ul>{totals}</ul>
            <p>Latest:</p>
            <ul>{latest}</ul>
            """;
    }

    private static string RenderDebts(ReminderDigest digest)
    {
        if (digest.Debts is not { } debts)
        {
            return string.Empty;
        }

        var now = digest.Now;
        var overdue = debts.Items.Count(d => d.Deadline < now);
        var dueSoon = debts.Items.Count(d => d.Deadline >= now && d.Deadline <= now.AddDays(7));

        var lines = string.Concat(debts.Items.Select(d =>
            $"<li>{Encode(d.Item)}: {Money(d.Amount, d.Currency.ToString())}{DeadlineLabel(d.Deadline, now)}</li>"));

        return $"""
            <h3>Debts</h3>
            <p>You have <strong>{debts.Items.Count.ToString(Culture)}</strong> unpaid debt(s):</p>
            <ul>{lines}</ul>
            {(overdue > 0 ? $"<p style=\"color:red\"><strong>{overdue.ToString(Culture)} debt(s) are overdue!</strong></p>" : string.Empty)}
            {(dueSoon > 0 ? $"<p style=\"color:orange\"><strong>{dueSoon.ToString(Culture)} debt(s) are due within 7 days.</strong></p>" : string.Empty)}
            """;
    }

    private static string DeadlineLabel(DateTimeOffset? deadline, DateTimeOffset now)
    {
        if (deadline is not { } value)
        {
            return string.Empty;
        }

        var text = $" (due {value.ToString("dd MMM yyyy", Culture)})";

        if (value < now)
        {
            return $"{text} — <strong style=\"color:red\">OVERDUE</strong>";
        }

        if (value <= now.AddDays(7))
        {
            return $"{text} — <strong style=\"color:orange\">Due soon</strong>";
        }

        return text;
    }

    private static string TotalLabel(DigestBankTotal total)
    {
        if (total.Type != BankTransactionType.Transfer)
        {
            return $"{total.Type.ToLabel()}s";
        }

        var direction = total.Direction == BankTransactionDirection.Incoming ? "in" : "out";

        return $"Transfers {direction}";
    }

    private static string Signed(BankTransactionDirection direction, decimal amount, string currency)
        => $"{(direction == BankTransactionDirection.Incoming ? "+" : "-")}{Money(amount, currency)}";

    private static string Money(decimal amount, string currency)
        => $"{amount.ToString("N2", Culture)} {Encode(currency)}".TrimEnd();

    private static string Encode(string? value)
        => WebUtility.HtmlEncode(value ?? string.Empty);
}
