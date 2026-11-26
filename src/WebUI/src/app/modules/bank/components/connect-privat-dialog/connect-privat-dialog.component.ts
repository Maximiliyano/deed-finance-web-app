import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ValidationErrors, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SharedModule } from '../../../../shared/shared.module';
import { BankConnectionService } from '../../services/bank-connection.service';

type ConnectMode = 'personal' | 'business';

const normalizeIban = (value: string): string => value.replace(/\s/g, '').toUpperCase();
const normalizeCard = (value: string): string => value.replace(/\D/g, '');

const ibanValidator = (control: AbstractControl<string>): ValidationErrors | null => {
  const iban = normalizeIban(control.value ?? '');

  return !iban || /^UA[0-9A-Z]{27}$/.test(iban) ? null : { iban: true };
};

const luhn = (digits: string): boolean => {
  let sum = 0;

  for (let i = 0; i < digits.length; i++) {
    let digit = Number(digits[digits.length - 1 - i]);

    if (i % 2 === 1) {
      digit *= 2;
      digit -= digit > 9 ? 9 : 0;
    }

    sum += digit;
  }

  return sum % 10 === 0;
};

const cardValidator = (control: AbstractControl<string>): ValidationErrors | null => {
  const digits = normalizeCard(control.value ?? '');

  return digits.length === 16 && luhn(digits) ? null : { card: true };
};

@Component({
  selector: 'app-connect-privat-dialog',
  templateUrl: './connect-privat-dialog.component.html',
  styleUrl: './connect-privat-dialog.component.scss',
  standalone: true,
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConnectPrivatDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject<DialogRef<boolean>>(DialogRef);
  private readonly bankConnection = inject(BankConnectionService);
  private readonly popup = inject(PopupMessageService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly personalForm = this.fb.nonNullable.group({
    merchantId: ['', [Validators.required, Validators.pattern(/^\d{1,32}$/)]],
    password: ['', [Validators.required, Validators.maxLength(128)]],
    card: ['', [Validators.required, cardValidator]]
  });

  protected readonly businessForm = this.fb.nonNullable.group({
    token: ['', [Validators.required, Validators.maxLength(512)]],
    iban: ['', [ibanValidator]]
  });

  protected readonly mode = signal<ConnectMode>('personal');
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly secretVisible = signal(false);

  protected get activeForm() {
    return this.mode() === 'personal' ? this.personalForm : this.businessForm;
  }

  protected setMode(mode: ConnectMode): void {
    this.mode.set(mode);
    this.error.set(null);
    this.secretVisible.set(false);
  }

  protected async paste(): Promise<void> {
    try {
      const text = await navigator.clipboard.readText();
      this.businessForm.controls.token.setValue(text.trim());
      this.businessForm.controls.token.markAsDirty();
    } catch {
      this.popup.info('Clipboard is not available. Paste the token into the field manually.');
    }
  }

  protected toggleSecretVisibility(): void {
    this.secretVisible.update(value => !value);
  }

  protected submit(): void {
    if (this.activeForm.invalid || this.submitting()) return;

    this.submitting.set(true);
    this.error.set(null);

    this.request()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.popup.success('Privat24 connected');
          this.dialogRef.close(true);
        },
        error: (response: HttpErrorResponse) => {
          this.submitting.set(false);
          this.error.set(
            response.error?.errors?.[0]?.message ?? 'Could not connect. Check the details you entered and try again.'
          );
        }
      });
  }

  protected cancel(): void {
    this.dialogRef.close(false);
  }

  private request(): Observable<unknown> {
    if (this.mode() === 'personal') {
      const { merchantId, password, card } = this.personalForm.getRawValue();

      return this.bankConnection.connectPrivatCard({
        merchantId: merchantId.trim(),
        password,
        cardNumber: normalizeCard(card)
      });
    }

    const { token, iban } = this.businessForm.getRawValue();

    return this.bankConnection.connectPrivat({ token: token.trim(), iban: normalizeIban(iban) });
  }
}
