import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, Validators } from '@angular/forms';
import { concatMap, from, toArray } from 'rxjs';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SharedModule } from '../../../../shared/shared.module';
import { PrivatAccount } from '../../models/bank.model';
import { BankConnectionService } from '../../services/bank-connection.service';

const DEFAULT_LABEL = 'Business';
const MAX_LABEL = 64;

@Component({
  selector: 'app-connect-privat-dialog',
  templateUrl: './connect-privat-dialog.component.html',
  styleUrl: '../../styles/connect-dialog.scss',
  standalone: true,
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConnectPrivatDialogComponent {
  private readonly data = inject<{ label?: string } | null>(DIALOG_DATA, { optional: true });
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject<DialogRef<boolean>>(DialogRef);
  private readonly bankConnection = inject(BankConnectionService);
  private readonly popup = inject(PopupMessageService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly form = this.fb.nonNullable.group({
    label: [this.data?.label ?? DEFAULT_LABEL, [Validators.maxLength(MAX_LABEL)]],
    token: ['', [Validators.required, Validators.maxLength(512)]]
  });

  protected readonly accounts = signal<PrivatAccount[] | null>(null);
  protected readonly selected = signal<string[]>([]);
  protected readonly loading = signal(false);
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly tokenVisible = signal(false);

  protected readonly allSelected = computed(() => {
    const list = this.accounts() ?? [];

    return list.length > 0 && this.selected().length === list.length;
  });

  protected get hasAccounts(): boolean {
    return (this.accounts()?.length ?? 0) > 0;
  }

  protected async paste(): Promise<void> {
    try {
      const text = await navigator.clipboard.readText();
      this.form.controls.token.setValue(text.trim());
      this.form.controls.token.markAsDirty();
      this.tokenChanged();
    } catch {
      this.popup.info('Clipboard is not available. Paste the token into the field manually.');
    }
  }

  protected toggleTokenVisibility(): void {
    this.tokenVisible.update(value => !value);
  }

  protected tokenChanged(): void {
    this.accounts.set(null);
    this.selected.set([]);
    this.error.set(null);
  }

  protected isSelected(iban: string): boolean {
    return this.selected().includes(iban);
  }

  protected toggle(iban: string): void {
    this.selected.update(list => (list.includes(iban) ? list.filter(item => item !== iban) : [...list, iban]));
  }

  protected toggleAll(): void {
    this.selected.set(this.allSelected() ? [] : (this.accounts() ?? []).map(account => account.iban));
  }

  protected accountName(account: PrivatAccount): string {
    return account.name || `${account.currency} account`.trim();
  }

  protected accountBalance(account: PrivatAccount): string {
    return account.balance === null ? '' : `${account.balance.toFixed(2)} ${account.currency}`.trim();
  }

  protected loadAccounts(): void {
    const token = this.form.controls.token;

    if (token.invalid || this.loading()) return;

    this.loading.set(true);
    this.error.set(null);

    this.bankConnection
      .privatAccounts(token.value.trim())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: accounts => {
          this.loading.set(false);
          this.accounts.set(accounts);
          this.selected.set(accounts.map(account => account.iban));

          if (accounts.length === 0) {
            this.error.set('No accounts were found for this token. Check that it is an Autoclient token.');
          }
        },
        error: (response: HttpErrorResponse) => {
          this.loading.set(false);
          this.error.set(this.message(response, 'Could not load accounts. Check the token.'));
        }
      });
  }

  protected submit(): void {
    if (this.submitting()) return;

    if (this.accounts() === null) {
      this.loadAccounts();
      return;
    }

    if (this.selected().length > 0) {
      this.connect(this.selected());
    }
  }

  protected cancel(): void {
    this.dialogRef.close(false);
  }

  private connect(ibans: string[]): void {
    const { label, token } = this.form.getRawValue();
    const base = label.trim() || DEFAULT_LABEL;

    this.submitting.set(true);
    this.error.set(null);

    from(ibans)
      .pipe(
        concatMap(iban =>
          this.bankConnection.connectPrivat({
            label: ibans.length > 1 ? `${base} ${iban.slice(-4)}`.slice(0, MAX_LABEL) : base,
            token: token.trim(),
            iban,
            currency: this.accounts()?.find(account => account.iban === iban)?.currency
          })
        ),
        toArray(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () => {
          this.popup.success(ibans.length > 1 ? `${ibans.length} accounts connected` : 'Privat24 connected');
          this.dialogRef.close(true);
        },
        error: (response: HttpErrorResponse) => {
          this.submitting.set(false);
          this.error.set(
            this.message(response, 'Could not connect. Check the token. Accounts connected so far are kept.')
          );
        }
      });
  }

  private message(response: HttpErrorResponse, fallback: string): string {
    return response.error?.errors?.[0]?.message ?? fallback;
  }
}
