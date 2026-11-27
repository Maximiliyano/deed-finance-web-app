import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { NumericInputDirective } from '../../../../shared/directives/numeric-input.directive';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { Exchange } from '../../../../core/models/exchange-model';
import { convertCurrency } from '../../../../shared/utils/currency-conversion.util';
import { PayUtilityBillRequest, UtilityBillResponse } from '../../models/utility-bill';

export interface PayUtilityBillDialogData {
  bill: UtilityBillResponse;
  exchanges: Exchange[];
}

@Component({
  selector: 'app-pay-utility-bill-dialog',
  templateUrl: './pay-utility-bill-dialog.component.html',
  styleUrl: './pay-utility-bill-dialog.component.scss',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NumericInputDirective],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PayUtilityBillDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject<DialogRef<PayUtilityBillRequest | null>>(DialogRef);
  private readonly data = inject<PayUtilityBillDialogData>(DIALOG_DATA);
  private readonly destroyRef = inject(DestroyRef);

  form!: FormGroup;
  bill!: UtilityBillResponse;
  capitalCurrency: string | null = null;
  metered = false;
  previousReading = 0;

  ngOnInit(): void {
    this.bill = this.data.bill;
    this.capitalCurrency =
      this.bill.capitalCurrency && this.bill.capitalCurrency !== this.bill.currency ? this.bill.capitalCurrency : null;
    this.metered = this.bill.unitPrice != null;
    this.previousReading = this.bill.previousReading ?? 0;
    const today = new Date().toISOString().slice(0, 10);
    this.form = this.fb.group({
      amount: [this.metered ? 0 : this.bill.estimatedAmount, [Validators.required, Validators.min(0.01)]],
      paymentDate: [today, Validators.required],
      meterReading: [null, this.metered ? [Validators.required, this.abovePrevious] : []]
    });

    if (this.metered) {
      this.form.controls['meterReading'].valueChanges
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => this.form.controls['amount'].setValue(this.consumptionAmount));
    }
  }

  get consumption(): number {
    const reading = Number(this.form.controls['meterReading'].value);

    return Number.isFinite(reading) && reading > this.previousReading ? reading - this.previousReading : 0;
  }

  get consumptionAmount(): number {
    return Math.round(this.consumption * (this.bill.unitPrice ?? 0) * 100) / 100;
  }

  get convertedAmount(): number | null {
    const amount = Number(this.form.value.amount);

    if (!this.capitalCurrency || !this.data.exchanges.length || !(amount > 0)) return null;

    return convertCurrency(amount, this.bill.currency, this.capitalCurrency, this.data.exchanges);
  }

  submit(): void {
    if (this.form.invalid) return;
    const val = this.form.value;
    this.dialogRef.close({
      amount: val.amount,
      paymentDate: val.paymentDate ? new Date(val.paymentDate).toISOString() : null,
      meterReading: this.metered ? Number(val.meterReading) : null
    });
  }

  private readonly abovePrevious = (control: AbstractControl): ValidationErrors | null => {
    const reading = Number(control.value);

    return Number.isFinite(reading) && reading > this.previousReading ? null : { belowPrevious: true };
  };

  cancel(): void {
    this.dialogRef.close(null);
  }
}
