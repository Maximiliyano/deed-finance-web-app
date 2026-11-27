import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ValidationErrors, Validators } from '@angular/forms';
import { CURRENCIES, CURRENCY_DEFAULT } from '../../../../core/types/currency-type';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SharedModule } from '../../../../shared/shared.module';
import { PrivatMailConnection } from '../../models/bank.model';
import { BankConnectionService } from '../../services/bank-connection.service';

const DEFAULT_LABEL = 'Privat24';
const MAX_LABEL = 64;
const IBAN_PATTERN = /^UA[0-9A-Z]{27}$/;

function normalizeIban(value: string): string {
  return value.replace(/\s+/g, '').toUpperCase();
}

function ibanValidator(control: AbstractControl<string>): ValidationErrors | null {
  return IBAN_PATTERN.test(normalizeIban(control.value ?? '')) ? null : { iban: true };
}

@Component({
  selector: 'app-connect-privat-mail-dialog',
  templateUrl: './connect-privat-mail-dialog.component.html',
  styleUrl: '../../styles/connect-dialog.scss',
  standalone: true,
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConnectPrivatMailDialogComponent {
  private readonly data = inject<{ addressTemplate: string }>(DIALOG_DATA);
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject<DialogRef<number | null>>(DialogRef);
  private readonly bankConnection = inject(BankConnectionService);
  private readonly popup = inject(PopupMessageService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly currencies = CURRENCIES;

  protected readonly form = this.fb.nonNullable.group({
    label: [DEFAULT_LABEL, [Validators.maxLength(MAX_LABEL)]],
    iban: ['', [Validators.required, ibanValidator]],
    currency: [CURRENCY_DEFAULT as string, [Validators.required]]
  });

  protected readonly created = signal<PrivatMailConnection | null>(null);
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly address = computed(() => {
    const connection = this.created();

    return connection ? this.data.addressTemplate.replace('{key}', connection.mailKey) : null;
  });

  protected submit(): void {
    if (this.form.invalid || this.submitting() || this.created()) return;

    const { label, iban, currency } = this.form.getRawValue();

    this.submitting.set(true);
    this.error.set(null);

    this.bankConnection
      .connectPrivatMail({ label: label.trim() || DEFAULT_LABEL, iban: normalizeIban(iban), currency })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: connection => {
          this.submitting.set(false);
          this.created.set(connection);
        },
        error: (response: HttpErrorResponse) => {
          this.submitting.set(false);
          this.error.set(response.error?.errors?.[0]?.message ?? 'Could not create the address. Try again.');
        }
      });
  }

  protected async copy(): Promise<void> {
    const address = this.address();

    if (!address) return;

    try {
      await navigator.clipboard.writeText(address);
      this.popup.success('Address copied');
    } catch {
      this.popup.info('Clipboard is not available. Select the address and copy it manually.');
    }
  }

  protected selectAll(event: Event): void {
    (event.target as HTMLInputElement).select();
  }

  protected done(): void {
    this.dialogRef.close(null);
  }

  protected linkCapital(): void {
    this.dialogRef.close(this.created()?.id ?? null);
  }

  protected cancel(): void {
    this.dialogRef.close(null);
  }
}
