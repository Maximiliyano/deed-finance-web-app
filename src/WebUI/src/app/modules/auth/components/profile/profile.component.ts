import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../services/auth-service';

import { SharedModule } from "../../../../shared/shared.module";
import { DialogService } from '../../../../shared/components/dialogs/services/dialog.service';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { ConfirmDialogComponent } from '../../../../shared/components/dialogs/confirm-dialog/confirm-dialog.component';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { User } from '../../models/user';
import { UserSettingsService } from '../../../home/services/user-settings.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CURRENCIES, CURRENCY_DEFAULT } from '../../../../core/types/currency-type';
import { UserSettings } from '../../../home/models/user-settings.model';

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
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly dialogRef = inject<DialogRef<void> | null>(DialogRef, { optional: true });
  private readonly destroyRef = inject(DestroyRef);

  user: User | null;
  settingsForm!: FormGroup;
  savingSettings = false;
  currencies = CURRENCIES;
  notificationForm!: FormGroup;
  cronPresets = [
    { label: 'Daily at 9:00 AM', value: '0 0 9 * * ?' },
    { label: 'Daily at 8:00 PM', value: '0 0 20 * * ?' },
    { label: 'Every Monday at 9:00 AM', value: '0 0 9 ? * MON' },
    { label: 'Every Friday at 6:00 PM', value: '0 0 18 ? * FRI' },
    { label: '1st of every month at 9:00 AM', value: '0 0 9 1 * ?' },
  ];

  ngOnInit(): void {
    this.settingsForm = this.fb.group({
        currency: [this.user?.currency ?? CURRENCY_DEFAULT, Validators.required]
    });

    this.notificationForm = this.fb.group({
      balanceReminderEnabled: [false],
      balanceReminderCron: ['0 0 9 * * ?'],
      expenseReminderEnabled: [false],
      expenseReminderCron: ['0 0 9 * * ?'],
      debtReminderEnabled: [false],
      debtReminderCron: ['0 0 9 * * ?'],
      emailNotificationsEnabled: [false]
    });

    this.authService.user$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(user => {
        this.user = user;
        document.title = `Deed - ${this.user?.fullname ?? ''} profile`;
        this.cdr.markForCheck();
      });

    this.userSettingsService.load()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings: UserSettings | null) => {
          if (settings) {
            this.settingsForm.patchValue({ currency: settings.currency });
            this.notificationForm.patchValue({
              balanceReminderEnabled: settings.balanceReminderEnabled ?? false,
              balanceReminderCron: settings.balanceReminderCron ?? '0 0 9 * * ?',
              expenseReminderEnabled: settings.expenseReminderEnabled ?? false,
              expenseReminderCron: settings.expenseReminderCron ?? '0 0 9 * * ?',
              debtReminderEnabled: settings.debtReminderEnabled ?? false,
              debtReminderCron: settings.debtReminderCron ?? '0 0 9 * * ?',
              emailNotificationsEnabled: settings.emailNotificationsEnabled ?? false
            });
            this.cdr.markForCheck();
          }
        }
      });
  }

  saveSettings(): void {
    if (this.settingsForm.invalid) return;
    this.savingSettings = true;
    const val = this.settingsForm.value;
    const notif = this.notificationForm.value;
    const payload = {
      currency: val.currency,
      balanceReminderEnabled: notif.balanceReminderEnabled,
      balanceReminderCron: notif.balanceReminderCron,
      expenseReminderEnabled: notif.expenseReminderEnabled,
      expenseReminderCron: notif.expenseReminderCron,
      debtReminderEnabled: notif.debtReminderEnabled,
      debtReminderCron: notif.debtReminderCron,
      emailNotificationsEnabled: notif.emailNotificationsEnabled
    };
    this.userSettingsService.patch(payload)
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

    dialogRef
      .afterClosed$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
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
