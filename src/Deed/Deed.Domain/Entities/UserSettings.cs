using Deed.Domain.Enums;

namespace Deed.Domain.Entities;

public sealed class UserSettings : Entity, IAuditableEntity
{
    public UserSettings() { }

    public UserSettings(int id) : base(id) { }

    public decimal Salary { get; set; }

    public CurrencyType Currency { get; set; }

    public bool BalanceReminderEnabled { get; set; }

    public string? BalanceReminderCron { get; set; }

    public bool ExpenseReminderEnabled { get; set; }

    public string? ExpenseReminderCron { get; set; }

    public bool DebtReminderEnabled { get; set; }

    public string? DebtReminderCron { get; set; }

    public bool EmailNotificationsEnabled { get; set; }

    public string? Email { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public string CreatedBy { get; init; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; init; }

    public string? UpdatedBy { get; init; }
}
