import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnInit,
  DestroyRef,
  inject,
  signal
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
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
import { BankConnection } from '../../../bank/models/bank.model';
import { BankConnectionService } from '../../../bank/services/bank-connection.service';
import { ConnectPrivatDialogComponent } from '../../../bank/components/connect-privat-dialog/connect-privat-dialog.component';

const DEFAULT_REMINDER_CRON = '0 0 9 * * ?';

const REMINDERS = [
  { key: 'balance', label: 'Balance reminder', hint: 'Get periodic summaries of your total balance' },
  { key: 'expense', label: 'Expense reminder', hint: 'Remind you to log your expenses' },
  { key: 'debt', label: 'Debt reminder', hint: 'Get alerts about upcoming or overdue debts' }
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
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly dialogRef = inject<DialogRef<void> | null>(DialogRef, { optional: true });
  private readonly destroyRef = inject(DestroyRef);

  readonly bankConnections = toSignal(this.bankConnectionService.connections$, { initialValue: [] });
  readonly syncing = signal(false);
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
      ...Object.fromEntries(
        REMINDERS.flatMap(({ key }) => [
          [`${key}ReminderEnabled`, [false]],
          [`${key}ReminderCron`, [DEFAULT_REMINDER_CRON]]
        ])
      ),
      emailNotificationsEnabled: [false],
      bankAlertsEmailEnabled: [false]
    });

    this.authService.user$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(user => {
      this.user = user;
      document.title = `Deed - ${this.user?.fullname ?? ''} profile`;
      this.cdr.markForCheck();
    });

    this.bankConnectionService.load().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();

    this.userSettingsService
      .load()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings: UserSettings | null) => {
          if (settings) {
            this.settingsForm.patchValue({ currency: settings.currency });
            this.notificationForm.patchValue({
              ...Object.fromEntries(
                REMINDERS.flatMap(({ key }) => [
                  [`${key}ReminderEnabled`, settings[`${key}ReminderEnabled`] ?? false],
                  [`${key}ReminderCron`, settings[`${key}ReminderCron`] ?? DEFAULT_REMINDER_CRON]
                ])
              ),
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

    dialogRef.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.cdr.markForCheck());
  }

  disconnectPrivat(connection: BankConnection): void {
    const dialogRef = this.dialogService.open(ConfirmDialogComponent, {
      data: {
        title: `Disconnect ${connection.label}`,
        message:
          'You will stop receiving alerts about new transactions on this account. The stored credentials will be deleted.',
        submitText: 'Disconnect',
        cancelText: 'Cancel',
        icon: 'warning'
      }
    });

    dialogRef.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((submit: boolean) => {
      if (!submit) return;

      this.bankConnectionService
        .disconnect(connection.id)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => this.popupMessageService.success(`${connection.label} disconnected`));
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
    this.save(this.notificationForm.value);
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
