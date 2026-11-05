using Deed.Domain.Enums;

namespace Deed.Api.Endpoints.UserSettings;

internal sealed record UpsertUserSettingsRequest(
    string Currency,
    bool BalanceReminderEnabled,
    string? BalanceReminderCron,
    bool ExpenseReminderEnabled,
    string? ExpenseReminderCron,
    bool DebtReminderEnabled,
    string? DebtReminderCron,
    bool EmailNotificationsEnabled);
