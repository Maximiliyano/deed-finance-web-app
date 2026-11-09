import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { PayUtilityBillRequest, UtilityBillResponse } from '../../models/utility-bill';

export interface PayUtilityBillDialogData {
  bill: UtilityBillResponse;
}

@Component({
  selector: 'app-pay-utility-bill-dialog',
  templateUrl: './pay-utility-bill-dialog.component.html',
  styleUrl: './pay-utility-bill-dialog.component.scss',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PayUtilityBillDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject<DialogRef<PayUtilityBillRequest | null>>(DialogRef);
  private readonly data = inject<PayUtilityBillDialogData>(DIALOG_DATA);

  form!: FormGroup;
  bill!: UtilityBillResponse;

  ngOnInit(): void {
    this.bill = this.data.bill;
    const today = new Date().toISOString().slice(0, 10);
    this.form = this.fb.group({
      amount: [this.bill.estimatedAmount, [Validators.required, Validators.min(0.01)]],
      paymentDate: [today, Validators.required]
    });
  }

  submit(): void {
    if (this.form.invalid) return;
    const val = this.form.value;
    this.dialogRef.close({
      amount: val.amount,
      paymentDate: val.paymentDate ? new Date(val.paymentDate).toISOString() : null
    });
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
