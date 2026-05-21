namespace Deed.Application.UserSettings.Responses;

public sealed record UserSettingsResponse(
    string Currency,
    bool BalanceReminderEnabled,
    string? BalanceReminderCron,
    bool ExpenseReminderEnabled,
    string? ExpenseReminderCron,
    bool DebtReminderEnabled,
    string? DebtReminderCron,
    bool EmailNotificationsEnabled
);
