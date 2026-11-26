import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ValidationErrors, Validators } from '@angular/forms';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SharedModule } from '../../../../shared/shared.module';
import { BankConnectionService } from '../../services/bank-connection.service';

const normalizeIban = (value: string): string => value.replace(/\s/g, '').toUpperCase();

const ibanValidator = (control: AbstractControl<string>): ValidationErrors | null =>
  /^UA[0-9A-Z]{27}$/.test(normalizeIban(control.value ?? '')) ? null : { iban: true };

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

  protected readonly form = this.fb.nonNullable.group({
    token: ['', [Validators.required, Validators.maxLength(512)]],
    iban: ['', [Validators.required, ibanValidator]]
  });

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly tokenVisible = signal(false);

  protected async paste(): Promise<void> {
    try {
      const text = await navigator.clipboard.readText();
      this.form.controls.token.setValue(text.trim());
      this.form.controls.token.markAsDirty();
    } catch {
      this.popup.info('Clipboard is not available. Paste the token into the field manually.');
    }
  }

  protected toggleTokenVisibility(): void {
    this.tokenVisible.update(value => !value);
  }

  protected submit(): void {
    if (this.form.invalid || this.submitting()) return;

    const { token, iban } = this.form.getRawValue();
    this.submitting.set(true);
    this.error.set(null);

    this.bankConnection
      .connectPrivat({ token: token.trim(), iban: normalizeIban(iban) })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.popup.success('Privat24 connected');
          this.dialogRef.close(true);
        },
        error: (response: HttpErrorResponse) => {
          this.submitting.set(false);
          this.error.set(
            response.error?.errors?.[0]?.message ?? 'Could not connect. Check the token and the account number.'
          );
        }
      });
  }

  protected cancel(): void {
    this.dialogRef.close(false);
  }
}
