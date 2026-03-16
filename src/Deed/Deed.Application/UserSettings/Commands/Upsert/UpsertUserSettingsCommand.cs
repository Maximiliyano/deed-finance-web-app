using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Enums;

namespace Deed.Application.UserSettings.Commands.Upsert;

public sealed record UpsertUserSettingsCommand(
    decimal Salary,
    CurrencyType Currency,
    bool BalanceReminderEnabled,
    string? BalanceReminderCron,
    bool ExpenseReminderEnabled,
    string? ExpenseReminderCron,
    bool DebtReminderEnabled,
    string? DebtReminderCron,
    bool EmailNotificationsEnabled)
    : ICommand;
