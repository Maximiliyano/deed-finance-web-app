export interface UserSettings {
  currency: string;
  balanceReminderEnabled?: boolean;
  balanceReminderCron?: string;
  expenseReminderEnabled?: boolean;
  expenseReminderCron?: string;
  debtReminderEnabled?: boolean;
  debtReminderCron?: string;
  emailNotificationsEnabled?: boolean;
}
