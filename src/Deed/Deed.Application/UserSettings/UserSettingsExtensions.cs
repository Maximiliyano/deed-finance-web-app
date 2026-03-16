using Deed.Application.UserSettings.Commands.Upsert;
using Deed.Application.UserSettings.Responses;
using Deed.Domain.Enums;
using DomainUserSettings = Deed.Domain.Entities.UserSettings;

namespace Deed.Application.UserSettings;

internal static class UserSettingsExtensions
{
    internal static UserSettingsResponse ToResponse(this DomainUserSettings settings)
        => new(
            settings.Salary,
            settings.Currency.ToString(),
            settings.BalanceReminderEnabled,
            settings.BalanceReminderCron,
            settings.ExpenseReminderEnabled,
            settings.ExpenseReminderCron,
            settings.DebtReminderEnabled,
            settings.DebtReminderCron,
            settings.EmailNotificationsEnabled);

    internal static DomainUserSettings ToEntity(this UpsertUserSettingsCommand command)
        => new()
        {
            Salary = command.Salary,
            Currency = command.Currency,
            BalanceReminderEnabled = command.BalanceReminderEnabled,
            BalanceReminderCron = command.BalanceReminderCron,
            ExpenseReminderEnabled = command.ExpenseReminderEnabled,
            ExpenseReminderCron = command.ExpenseReminderCron,
            DebtReminderEnabled = command.DebtReminderEnabled,
            DebtReminderCron = command.DebtReminderCron,
            EmailNotificationsEnabled = command.EmailNotificationsEnabled,
        };
}
