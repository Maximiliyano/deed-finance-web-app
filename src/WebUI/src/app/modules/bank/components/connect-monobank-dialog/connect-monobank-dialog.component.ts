import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, Validators } from '@angular/forms';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SharedModule } from '../../../../shared/shared.module';
import { MonobankAccount } from '../../models/bank.model';
import { BankConnectionService } from '../../services/bank-connection.service';

const DEFAULT_LABEL = 'Monobank';

@Component({
  selector: 'app-connect-monobank-dialog',
  templateUrl: './connect-monobank-dialog.component.html',
  styleUrl: '../../styles/connect-dialog.scss',
  standalone: true,
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConnectMonobankDialogComponent {
  private readonly data = inject<{ label?: string } | null>(DIALOG_DATA, { optional: true });
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject<DialogRef<boolean>>(DialogRef);
  private readonly bankConnection = inject(BankConnectionService);
  private readonly popup = inject(PopupMessageService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly form = this.fb.nonNullable.group({
    label: [this.data?.label ?? DEFAULT_LABEL, [Validators.maxLength(64)]],
    token: ['', [Validators.required, Validators.maxLength(256)]],
    accountId: ['']
  });

  protected readonly accounts = signal<MonobankAccount[] | null>(null);
  protected readonly loading = signal(false);
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly tokenVisible = signal(false);

  protected async paste(): Promise<void> {
    try {
      const text = await navigator.clipboard.readText();
      this.form.controls.token.setValue(text.trim());
      this.form.controls.token.markAsDirty();
      this.accounts.set(null);
    } catch {
      this.popup.info('Clipboard is not available. Paste the token into the field manually.');
    }
  }

  protected toggleTokenVisibility(): void {
    this.tokenVisible.update(value => !value);
  }

  protected tokenChanged(): void {
    this.accounts.set(null);
    this.error.set(null);
  }

  protected accountLabel(account: MonobankAccount): string {
    const card = account.maskedCard ?? account.iban.slice(-4);

    return `${card} · ${account.currency} · ${account.balance.toFixed(2)}`;
  }

  protected loadAccounts(): void {
    const token = this.form.controls.token;

    if (token.invalid || this.loading()) return;

    this.loading.set(true);
    this.error.set(null);

    this.bankConnection
      .monobankAccounts(token.value.trim())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: accounts => {
          this.loading.set(false);
          this.accounts.set(accounts);
          this.form.controls.accountId.setValue(accounts[0]?.id ?? '');

          if (accounts.length === 0) {
            this.error.set('No accounts were found for this token.');
          }
        },
        error: (response: HttpErrorResponse) => {
          this.loading.set(false);
          this.error.set(this.message(response, 'Could not load accounts. Check the token.'));
        }
      });
  }

  protected submit(): void {
    if (this.accounts() === null) {
      this.loadAccounts();
      return;
    }

    const { label, token, accountId } = this.form.getRawValue();
    const account = this.accounts()?.find(a => a.id === accountId);

    if (!account || this.form.invalid || this.submitting()) return;

    this.submitting.set(true);
    this.error.set(null);

    this.bankConnection
      .connectMonobank({
        label: label.trim(),
        token: token.trim(),
        accountId: account.id,
        iban: account.iban,
        currency: account.currency
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.popup.success('Monobank connected');
          this.dialogRef.close(true);
        },
        error: (response: HttpErrorResponse) => {
          this.submitting.set(false);
          this.error.set(this.message(response, 'Could not connect. Try again in a minute.'));
        }
      });
  }

  protected cancel(): void {
    this.dialogRef.close(false);
  }

  private message(response: HttpErrorResponse, fallback: string): string {
    return response.error?.errors?.[0]?.message ?? fallback;
  }
}
