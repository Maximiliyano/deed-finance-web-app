import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { NumericInputDirective } from '../../../../shared/directives/numeric-input.directive';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { Debt } from '../../models/debt.model';
import { CapitalResponse } from '../../../capital/models/capital-response';
import { DebtForm } from '../../models/debt-form';
import { CURRENCIES, CURRENCY_DEFAULT } from '../../../../core/types/currency-type';

export interface DebtDialogData {
  debt?: Debt;
  capitals: CapitalResponse[];
  me?: string;
  people?: string[];
}

type Party = 'source' | 'recipient';

const DEFAULT_ME = 'Me';

const pad = (value: number): string => String(value).padStart(2, '0');
const toDateInput = (date: Date): string => `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
const daysFromToday = (days: number): string => {
  const today = new Date();

  return toDateInput(new Date(today.getFullYear(), today.getMonth(), today.getDate() + days));
};

@Component({
  selector: 'app-debt-dialog',
  templateUrl: './debt-dialog.component.html',
  styleUrl: './debt-dialog.component.scss',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NumericInputDirective],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DebtDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject<DialogRef<DebtForm | null>>(DialogRef);
  private readonly data = inject<DebtDialogData>(DIALOG_DATA);
  private readonly destroyRef = inject(DestroyRef);

  form!: FormGroup;
  currencies = CURRENCIES;
  capitals: CapitalResponse[] = [];
  me = DEFAULT_ME;
  people: string[] = [];
  isEdit = false;
  showPayConfirm = false;

  readonly deadlineShortcuts = [
    { label: 'In a week', days: 7 },
    { label: 'In a month', days: 30 }
  ];

  protected readonly samePerson = signal(false);

  ngOnInit(): void {
    this.isEdit = !!this.data.debt;
    this.capitals = this.data.capitals ?? [];
    this.me = this.data.me?.trim() || DEFAULT_ME;
    this.people = [...new Set([this.me, ...(this.data.people ?? [])])];
    const d = this.data.debt;

    this.form = this.fb.group({
      item: [d?.item ?? '', [Validators.required, Validators.maxLength(64)]],
      amount: [d?.amount ?? null, [Validators.required, Validators.min(0.01)]],
      currency: [d?.currency ?? CURRENCY_DEFAULT, Validators.required],
      source: [d?.source ?? '', [Validators.required, Validators.maxLength(128)]],
      recipient: [d?.recipient ?? this.me, [Validators.required, Validators.maxLength(128)]],
      borrowedAt: [d?.borrowedAt ? d.borrowedAt.slice(0, 10) : daysFromToday(1), Validators.required],
      deadlineAt: [d?.deadlineAt ? d.deadlineAt.slice(0, 10) : ''],
      note: [d?.note ?? ''],
      isPaid: [d?.isPaid ?? false],
      capitalId: [d?.capitalId ?? null],
      payFromCapitalId: [d?.capitalId ?? null]
    });

    this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.samePerson.set(this.isSamePerson()));
    this.samePerson.set(this.isSamePerson());
  }

  get isPaidChecked(): boolean {
    return this.form.get('isPaid')?.value;
  }

  get wasPaid(): boolean {
    return !!this.data.debt?.isPaid;
  }

  isMe(party: Party): boolean {
    return this.normalized(party) === this.me.toLowerCase();
  }

  setMe(party: Party): void {
    this.form.get(party)?.setValue(this.me);
    this.form.get(party)?.markAsDirty();
  }

  swapParties(): void {
    const { source, recipient } = this.form.value;
    this.form.patchValue({ source: recipient, recipient: source });
    this.form.markAsDirty();
  }

  setDeadlineIn(days: number): void {
    const start = this.form.get('borrowedAt')?.value;
    const base = start ? new Date(`${start}T00:00:00`) : new Date();
    const deadline = new Date(base.getFullYear(), base.getMonth(), base.getDate() + days);

    this.form.get('deadlineAt')?.setValue(toDateInput(deadline));
    this.form.get('deadlineAt')?.markAsDirty();
  }

  get deadlineBeforeBorrowed(): boolean {
    const { borrowedAt, deadlineAt } = this.form.value;

    return !!borrowedAt && !!deadlineAt && deadlineAt < borrowedAt;
  }

  onPaidChange(): void {
    if (this.isPaidChecked && !this.wasPaid) {
      this.showPayConfirm = true;
    } else {
      this.showPayConfirm = false;
    }
  }

  submit(): void {
    if (this.form.invalid || this.samePerson() || this.deadlineBeforeBorrowed) return;
    const val = this.form.value;
    this.dialogRef.close({
      item: val.item,
      amount: val.amount,
      currency: val.currency,
      source: val.source.trim(),
      recipient: val.recipient.trim(),
      borrowedAt: val.borrowedAt || null,
      deadlineAt: val.deadlineAt || null,
      note: val.note || null,
      isPaid: val.isPaid,
      capitalId: val.capitalId || null,
      payFromCapitalId: val.isPaid && !this.wasPaid ? val.payFromCapitalId || null : null
    });
  }

  cancel(): void {
    this.dialogRef.close(null);
  }

  private normalized(party: Party): string {
    return String(this.form.get(party)?.value ?? '').trim().toLowerCase();
  }

  private isSamePerson(): boolean {
    const source = this.normalized('source');

    return source !== '' && source === this.normalized('recipient');
  }
}
