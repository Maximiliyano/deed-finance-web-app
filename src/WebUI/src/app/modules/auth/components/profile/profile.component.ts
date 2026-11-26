import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnInit,
  DestroyRef,
  computed,
  inject,
  signal
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { switchMap } from 'rxjs';
import { AuthService } from '../../services/auth-service';

import { SharedModule } from '../../../../shared/shared.module';
import { DialogService } from '../../../../shared/components/dialogs/services/dialog.service';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { ConfirmDialogComponent } from '../../../../shared/components/dialogs/confirm-dialog/confirm-dialog.component';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { User } from '../../models/user';
import { UserSettingsService } from '../../../home/services/user-settings.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CURRENCIES, CURRENCY_DEFAULT } from '../../../../core/types/currency-type';
import { UserSettings } from '../../../home/models/user-settings.model';
import { BankAccount, BankConnection, BankProvider } from '../../../bank/models/bank.model';
import { BankAccountService } from '../../../bank/services/bank-account.service';
import { formatMoney } from '../../../bank/utils/bank-format';
import { BankConnectionService } from '../../../bank/services/bank-connection.service';
import { BankConnectionSettingsDialogComponent } from '../../../bank/components/bank-connection-settings-dialog/bank-connection-settings-dialog.component';
import { ConnectMonobankDialogComponent } from '../../../bank/components/connect-monobank-dialog/connect-monobank-dialog.component';
import { ConnectPrivatDialogComponent } from '../../../bank/components/connect-privat-dialog/connect-privat-dialog.component';

const BANK_PROVIDERS: { value: BankProvider; label: string }[] = [
  { value: 'PrivatAutoclient', label: 'Privat24 Business' },
  { value: 'Monobank', label: 'Monobank' }
];

const DEFAULT_REMINDER_CRON = '0 0 9 * * ?';

const REMINDERS = [
  { key: 'balance', label: 'Balance', hint: 'Total balance and every capital' },
  { key: 'expense', label: 'Expenses & incomes', hint: 'Totals for the period and top expense categories' },
  { key: 'debt', label: 'Debts', hint: 'Unpaid, overdue and soon-due debts' }
] as const;

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  standalone: true,
  styleUrl: './profile.component.scss',
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly dialogService = inject(DialogService);
  private readonly popupMessageService = inject(PopupMessageService);
  private readonly userSettingsService = inject(UserSettingsService);
  private readonly bankConnectionService = inject(BankConnectionService);
  private readonly bankAccountService = inject(BankAccountService);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly dialogRef = inject<DialogRef<void> | null>(DialogRef, { optional: true });
  private readonly destroyRef = inject(DestroyRef);

  readonly bankConnections = toSignal(this.bankConnectionService.connections$, { initialValue: [] });
  readonly bankAccounts = toSignal(this.bankAccountService.accounts$, { initialValue: [] });
  readonly availableAccounts = computed(() => this.bankAccounts().filter(account => account.connectionId === null));
  readonly connectedProviders = computed(() =>
    BANK_PROVIDERS.filter(
      option =>
        this.bankConnections().some(connection => connection.provider === option.value) ||
        this.bankAccounts().some(account => account.provider === option.value)
    )
  );
  readonly syncing = signal(false);
  readonly refreshing = signal(false);
  readonly connectingId = signal<number | null>(null);
  user: User | null;
  settingsForm!: FormGroup;
  savingSettings = false;
  currencies = CURRENCIES;
  notificationForm!: FormGroup;
  reminders = REMINDERS;
  cronPresets = [
    { label: 'Daily at 9:00 AM', value: DEFAULT_REMINDER_CRON },
    { label: 'Daily at 8:00 PM', value: '0 0 20 * * ?' },
    { label: 'Every Monday at 9:00 AM', value: '0 0 9 ? * MON' },
    { label: 'Every Friday at 6:00 PM', value: '0 0 18 ? * FRI' },
    { label: '1st of every month at 9:00 AM', value: '0 0 9 1 * ?' }
  ];

  ngOnInit(): void {
    this.settingsForm = this.fb.group({
      currency: [this.user?.currency ?? CURRENCY_DEFAULT, Validators.required]
    });

    this.notificationForm = this.fb.group({
      ...Object.fromEntries(REMINDERS.map(({ key }) => [`${key}ReminderEnabled`, [false]])),
      summaryCron: [DEFAULT_REMINDER_CRON],
      emailNotificationsEnabled: [false],
      bankAlertsEmailEnabled: [false]
    });

    this.authService.user$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(user => {
      this.user = user;
      document.title = `Deed - ${this.user?.fullname ?? ''} profile`;
      this.cdr.markForCheck();
    });

    this.bankConnectionService.load().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
    this.bankAccountService.load().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();

    this.userSettingsService
      .load()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings: UserSettings | null) => {
          if (settings) {
            this.settingsForm.patchValue({ currency: settings.currency });
            this.notificationForm.patchValue({
              ...Object.fromEntries(
                REMINDERS.map(({ key }) => [`${key}ReminderEnabled`, settings[`${key}ReminderEnabled`] ?? false])
              ),
              summaryCron: this.resolveSummaryCron(settings),
              emailNotificationsEnabled: settings.emailNotificationsEnabled ?? false,
              bankAlertsEmailEnabled: settings.bankAlertsEmailEnabled ?? false
            });
            this.cdr.markForCheck();
          }
        }
      });
  }

  connectPrivat(existing?: BankConnection): void {
    const dialogRef = this.dialogService.open(ConnectPrivatDialogComponent, {
      data: existing ? { label: existing.label } : null
    });

    dialogRef.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((connected: boolean) => {
      if (connected) {
        this.bankAccountService.refresh().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
      }

      this.cdr.markForCheck();
    });
  }

  connectMonobank(existing?: BankConnection): void {
    const dialogRef = this.dialogService.open(ConnectMonobankDialogComponent, {
      data: existing ? { label: existing.label } : null
    });

    dialogRef.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.cdr.markForCheck());
  }

  reconnect(connection: BankConnection): void {
    if (connection.provider === 'Monobank') {
      this.connectMonobank(connection);
    } else {
      this.connectPrivat(connection);
    }
  }

  openConnectionSettings(connection: BankConnection): void {
    this.dialogService
      .open(BankConnectionSettingsDialogComponent, { data: connection })
      .afterClosed$.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.cdr.markForCheck());
  }

  balanceOf(connection: BankConnection): string | null {
    const account = this.bankAccounts().find(item => item.connectionId === connection.id);

    return account?.balance == null ? null : this.money(account.balance, account.currency);
  }

  money(amount: number, currency: string): string {
    return formatMoney(amount, currency);
  }

  accountName(account: BankAccount): string {
    return account.name || `${account.currency} account`.trim();
  }

  refreshAccounts(): void {
    if (this.refreshing()) return;

    this.refreshing.set(true);

    this.bankAccountService
      .refresh()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.refreshing.set(false);
          this.popupMessageService.info('Accounts refreshed');
        },
        error: () => this.refreshing.set(false)
      });
  }

  connectAvailable(account: BankAccount): void {
    if (this.connectingId() !== null) return;

    this.connectingId.set(account.id);

    this.bankAccountService
      .connect(account.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.connectingId.set(null);
          this.popupMessageService.success(`${this.accountName(account)} connected`);
          this.bankConnectionService.load().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
        },
        error: () => this.connectingId.set(null)
      });
  }

  providerName(connection: BankConnection): string {
    return connection.provider === 'Monobank' ? 'Monobank' : 'Privat24 Business';
  }

  toggleExcluded(connection: BankConnection): void {
    this.bankConnectionService
      .setExcluded(connection.id, !connection.isExcluded)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() =>
        this.popupMessageService.info(
          connection.isExcluded
            ? `${connection.label} is counted on the bank page again`
            : `${connection.label} is hidden from the bank page`
        )
      );
  }

  disconnectProvider(option: { value: BankProvider; label: string }): void {
    const dialogRef = this.dialogService.open(ConfirmDialogComponent, {
      data: {
        title: `Disconnect ${option.label}`,
        message: `Every ${option.label} account, its stored token and its imported transactions will be removed from Deed and from the bank page. Your accounts at the bank are not affected.`,
        submitText: 'Disconnect',
        cancelText: 'Cancel',
        icon: 'warning'
      }
    });

    dialogRef.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((submit: boolean) => {
      if (!submit) return;

      this.bankConnectionService
        .disconnectProvider(option.value)
        .pipe(
          switchMap(() => this.bankAccountService.load()),
          takeUntilDestroyed(this.destroyRef)
        )
        .subscribe(() => this.popupMessageService.success(`${option.label} disconnected`));
    });
  }

  syncNow(): void {
    if (this.syncing()) return;

    this.syncing.set(true);

    this.bankConnectionService
      .syncNow()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: created => {
          this.syncing.set(false);
          this.popupMessageService.info(
            created > 0 ? `${created} new transaction${created === 1 ? '' : 's'} found` : 'No new transactions'
          );
        },
        error: () => this.syncing.set(false)
      });
  }

  saveCurrency(): void {
    if (this.settingsForm.invalid) return;
    this.save({ currency: this.settingsForm.value.currency });
  }

  saveNotifications(): void {
    const { summaryCron, ...rest } = this.notificationForm.value;

    this.save({
      ...rest,
      ...Object.fromEntries(REMINDERS.map(({ key }) => [`${key}ReminderCron`, summaryCron]))
    });
  }

  private resolveSummaryCron(settings: UserSettings): string {
    const enabled = REMINDERS.find(({ key }) => settings[`${key}ReminderEnabled`] && settings[`${key}ReminderCron`]);

    return (enabled && settings[`${enabled.key}ReminderCron`]) || settings.balanceReminderCron || DEFAULT_REMINDER_CRON;
  }

  private save(changes: Partial<UserSettings>): void {
    this.savingSettings = true;
    this.userSettingsService
      .patch(changes)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.savingSettings = false;
          this.cdr.markForCheck();
          this.popupMessageService.success('Settings saved');
        },
        error: () => {
          this.savingSettings = false;
          this.cdr.markForCheck();
          this.popupMessageService.error('Failed to save settings');
        }
      });
  }

  handleLogout(): void {
    const dialogRef = this.dialogService.open(ConfirmDialogComponent, {
      data: {
        title: 'Log out',
        message: 'Are you sure you want to log out your account?',
        submitText: 'Yes',
        cancelText: 'Cancel',
        icon: 'warning'
      }
    });

    dialogRef.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (submit: boolean) => {
        if (submit) {
          this.authService.logout();
          this.popupMessageService.warning('You logged out your account.');
          this.dialogRef?.close();
        }
      }
    });
  }
}
