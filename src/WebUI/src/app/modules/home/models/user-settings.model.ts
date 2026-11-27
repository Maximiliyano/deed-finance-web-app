export type ThemeSetting = 'System' | 'Dark' | 'Light';

export interface UserSettings {
  currency: string;
  theme?: ThemeSetting;
  balanceReminderEnabled?: boolean;
  balanceReminderCron?: string;
  expenseReminderEnabled?: boolean;
  expenseReminderCron?: string;
  debtReminderEnabled?: boolean;
  debtReminderCron?: string;
  emailNotificationsEnabled?: boolean;
  bankAlertsEmailEnabled?: boolean;
}
