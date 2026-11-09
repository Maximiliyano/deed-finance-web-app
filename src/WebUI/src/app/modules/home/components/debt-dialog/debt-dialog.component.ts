import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { Debt } from '../../models/debt.model';
import { CapitalResponse } from '../../../capital/models/capital-response';
import { DebtForm } from '../../models/debt-form';
import { CURRENCIES, CURRENCY_DEFAULT } from '../../../../core/types/currency-type';

export interface DebtDialogData {
  debt?: Debt;
  capitals: CapitalResponse[];
}

@Component({
  selector: 'app-debt-dialog',
  templateUrl: './debt-dialog.component.html',
  styleUrl: './debt-dialog.component.scss',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DebtDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject<DialogRef<DebtForm | null>>(DialogRef);
  private readonly data = inject<DebtDialogData>(DIALOG_DATA);

  form!: FormGroup;
  currencies = CURRENCIES;
  capitals: CapitalResponse[] = [];
  isEdit = false;
  showPayConfirm = false;

  ngOnInit(): void {
    this.isEdit = !!this.data.debt;
    this.capitals = this.data.capitals ?? [];
    const d = this.data.debt;

    this.form = this.fb.group({
      item: [d?.item ?? '', [Validators.required, Validators.maxLength(64)]],
      amount: [d?.amount ?? 0, [Validators.required, Validators.min(0.01)]],
      currency: [d?.currency ?? CURRENCY_DEFAULT, Validators.required],
      source: [d?.source ?? '', [Validators.required, Validators.maxLength(128)]],
      recipient: [d?.recipient ?? '', [Validators.required, Validators.maxLength(128)]],
      borrowedAt: [d?.borrowedAt ? d.borrowedAt.slice(0, 10) : '', Validators.required],
      deadlineAt: [d?.deadlineAt ? d.deadlineAt.slice(0, 10) : ''],
      note: [d?.note ?? ''],
      isPaid: [d?.isPaid ?? false],
      capitalId: [d?.capitalId ?? null],
      payFromCapitalId: [d?.capitalId ?? null]
    });
  }

  get isPaidChecked(): boolean {
    return this.form.get('isPaid')?.value;
  }

  get wasPaid(): boolean {
    return !!this.data.debt?.isPaid;
  }

  onPaidChange(): void {
    if (this.isPaidChecked && !this.wasPaid) {
      this.showPayConfirm = true;
    } else {
      this.showPayConfirm = false;
    }
  }

  submit(): void {
    if (this.form.invalid) return;
    const val = this.form.value;
    this.dialogRef.close({
      item: val.item,
      amount: val.amount,
      currency: val.currency,
      source: val.source,
      recipient: val.recipient,
      borrowedAt: val.borrowedAt || null,
      deadlineAt: val.deadlineAt || null,
      note: val.note || null,
      isPaid: val.isPaid,
      capitalId: val.capitalId || null,
      payFromCapitalId: (val.isPaid && !this.wasPaid) ? (val.payFromCapitalId || null) : null
    });
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
