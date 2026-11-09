using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.UserSettings.Commands.Upsert;

public sealed record UpsertUserSettingsCommand(
    string Currency,
    bool BalanceReminderEnabled,
    string? BalanceReminderCron,
    bool ExpenseReminderEnabled,
    string? ExpenseReminderCron,
    bool DebtReminderEnabled,
    string? DebtReminderCron,
    bool EmailNotificationsEnabled)
    : ICommand;
