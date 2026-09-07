import { Component, Inject, OnInit } from '@angular/core';
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
  imports: [CommonModule, ReactiveFormsModule]
})
export class PayUtilityBillDialogComponent implements OnInit {
  form!: FormGroup;
  bill!: UtilityBillResponse;

  constructor(
    private readonly fb: FormBuilder,
    private readonly dialogRef: DialogRef<PayUtilityBillRequest | null>,
    @Inject(DIALOG_DATA) private readonly data: PayUtilityBillDialogData
  ) {}

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
