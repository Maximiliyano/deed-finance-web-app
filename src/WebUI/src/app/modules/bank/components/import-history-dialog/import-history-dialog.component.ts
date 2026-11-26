import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder } from '@angular/forms';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { SharedModule } from '../../../../shared/shared.module';
import { BankHistoryImportResponse } from '../../models/bank.model';
import { BankConnectionService } from '../../services/bank-connection.service';

type Preset = '30d' | '3m' | '6m' | '1y' | '3y' | 'all' | 'custom';

const MAX_YEARS = 10;

const PRESETS: { value: Preset; label: string }[] = [
  { value: '30d', label: 'Last 30 days' },
  { value: '3m', label: 'Last 3 months' },
  { value: '6m', label: 'Last 6 months' },
  { value: '1y', label: 'Last year' },
  { value: '3y', label: 'Last 3 years' },
  { value: 'all', label: 'All time' },
  { value: 'custom', label: 'Custom period' }
];

const toInputDate = (date: Date): string =>
  `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;

const monthsAgo = (months: number): Date => {
  const date = new Date();
  date.setMonth(date.getMonth() - months);
  return date;
};

@Component({
  selector: 'app-import-history-dialog',
  templateUrl: './import-history-dialog.component.html',
  styleUrl: './import-history-dialog.component.scss',
  standalone: true,
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ImportHistoryDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject<DialogRef<BankHistoryImportResponse | null>>(DialogRef);
  private readonly bankConnection = inject(BankConnectionService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly presets = PRESETS;
  protected readonly today = toInputDate(new Date());
  protected readonly earliest = toInputDate(monthsAgo(MAX_YEARS * 12));

  protected readonly form = this.fb.nonNullable.group({
    preset: ['3m' as Preset],
    from: [toInputDate(monthsAgo(3))],
    to: [toInputDate(new Date())]
  });

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected get custom(): boolean {
    return this.form.controls.preset.value === 'custom';
  }

  protected get customInvalid(): boolean {
    const { from, to } = this.form.getRawValue();

    return this.custom && (!from || !to || from > to || from < this.earliest || to > this.today);
  }

  protected submit(): void {
    if (this.customInvalid || this.submitting()) return;

    const range = this.range();

    this.submitting.set(true);
    this.error.set(null);

    this.bankConnection
      .importHistory(range)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: result => this.dialogRef.close(result),
        error: (response: HttpErrorResponse) => {
          this.submitting.set(false);
          this.error.set(response.error?.errors?.[0]?.message ?? 'Could not import the history.');
        }
      });
  }

  protected cancel(): void {
    this.dialogRef.close(null);
  }

  private range(): { from: string; to: string | null } {
    const { preset, from, to } = this.form.getRawValue();

    if (preset === 'custom') {
      const end = new Date(`${to}T23:59:59`);

      return {
        from: new Date(`${from}T00:00:00`).toISOString(),
        to: (end > new Date() ? new Date() : end).toISOString()
      };
    }

    const start = new Date();

    switch (preset) {
      case '30d':
        start.setDate(start.getDate() - 30);
        break;
      case '3m':
        start.setMonth(start.getMonth() - 3);
        break;
      case '6m':
        start.setMonth(start.getMonth() - 6);
        break;
      case '1y':
        start.setFullYear(start.getFullYear() - 1);
        break;
      case '3y':
        start.setFullYear(start.getFullYear() - 3);
        break;
      default:
        start.setFullYear(start.getFullYear() - MAX_YEARS);
        start.setDate(start.getDate() + 1);
    }

    return { from: start.toISOString(), to: null };
  }
}
