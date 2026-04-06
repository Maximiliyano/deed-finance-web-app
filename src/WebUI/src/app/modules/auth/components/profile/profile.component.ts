import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth-service';
import { Subject, takeUntil } from 'rxjs';
import { SharedModule } from "../../../../shared/shared.module";
import { DialogService } from '../../../../shared/components/dialogs/services/dialog.service';
import { ConfirmDialogComponent } from '../../../../shared/components/dialogs/confirm-dialog/confirm-dialog.component';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { User } from '../../models/user';
import { UserSettingsService } from '../../../home/services/user-settings.service';
import { UserSettings } from '../../../home/models/user-settings.model';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CurrencyType } from '../../../../core/types/currency-type';
import { ThemeService } from '../../../../core/services/theme.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  standalone: true,
  styleUrl: './profile.component.scss',
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileComponent implements OnInit, OnDestroy {
  user: User | null;
  editMode: boolean;
  settingsForm!: FormGroup;
  savingSettings = false;
  currencies = Object.keys(CurrencyType).filter(k => isNaN(Number(k)) && k !== 'None');
  notificationForm!: FormGroup;
  savingNotifications = false;
  cronPresets = [
    { label: 'Daily at 9:00 AM', value: '0 0 9 * * ?' },
    { label: 'Daily at 8:00 PM', value: '0 0 20 * * ?' },
    { label: 'Every Monday at 9:00 AM', value: '0 0 9 ? * MON' },
    { label: 'Every Friday at 6:00 PM', value: '0 0 18 ? * FRI' },
    { label: '1st of every month at 9:00 AM', value: '0 0 9 1 * ?' },
  ];

  private $unsubscribe = new Subject<void>();

  constructor(
    private readonly authService: AuthService,
    private readonly dialogService: DialogService,
    private readonly popupMessageService: PopupMessageService,
    private readonly userSettingsService: UserSettingsService,
    private readonly fb: FormBuilder,
    readonly themeService: ThemeService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.settingsForm = this.fb.group({
      salary: [0, [Validators.required, Validators.min(0)]],
      currency: ['UAH', Validators.required]
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
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe(user => {
        this.user = user;
        document.title = `Deed - ${this.user?.fullname ?? ''} profile`;
        this.cdr.markForCheck();
      });

    this.userSettingsService.get()
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe(settings => {
        if (settings) {
          this.settingsForm.patchValue({ salary: settings.salary, currency: settings.currency });
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
      });
  }

  ngOnDestroy(): void {
    this.$unsubscribe.next();
    this.$unsubscribe.complete();
  }

  saveSettings(): void {
    if (this.settingsForm.invalid) return;
    this.savingSettings = true;
    const val = this.settingsForm.value;
    const notif = this.notificationForm.value;
    const payload = {
      salary: val.salary,
      currency: CurrencyType[val.currency as keyof typeof CurrencyType] as unknown as number,
      balanceReminderEnabled: notif.balanceReminderEnabled,
      balanceReminderCron: notif.balanceReminderCron,
      expenseReminderEnabled: notif.expenseReminderEnabled,
      expenseReminderCron: notif.expenseReminderCron,
      debtReminderEnabled: notif.debtReminderEnabled,
      debtReminderCron: notif.debtReminderCron,
      emailNotificationsEnabled: notif.emailNotificationsEnabled
    };
    this.userSettingsService.upsert(payload as unknown as UserSettings)
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: () => {
          this.savingSettings = false;
          this.popupMessageService.success('Settings saved');
        },
        error: () => {
          this.savingSettings = false;
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
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: (submit: boolean) => {
          if (submit) {
            this.authService.logout();
            this.popupMessageService.warning('You logged out your account.');
          }
        }
      });
  }
}
