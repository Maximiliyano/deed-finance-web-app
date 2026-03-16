import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { CapitalResponse } from '../../../capital/models/capital-response';
import { Exchange } from '../../../../core/models/exchange-model';

export interface TransferDialogData {
  capitals: CapitalResponse[];
  exchanges: Exchange[];
}

@Component({
  selector: 'app-transfer-dialog',
  templateUrl: './transfer-dialog.component.html',
  styleUrl: './transfer-dialog.component.scss',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class TransferDialogComponent implements OnInit {
  form!: FormGroup;
  capitals: CapitalResponse[] = [];
  exchanges: Exchange[] = [];

  constructor(
    private readonly fb: FormBuilder,
    private readonly dialogRef: DialogRef<any>,
    @Inject(DIALOG_DATA) private readonly data: TransferDialogData
  ) {
    this.capitals = data.capitals;
    this.exchanges = data.exchanges ?? [];
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      sourceCapitalId: [null, Validators.required],
      destinationCapitalId: [null, Validators.required],
      amount: [null, [Validators.required, Validators.min(0.01)]],
      destinationAmount: [null, [Validators.required, Validators.min(0.01)]]
    });

    this.form.get('amount')!.valueChanges.subscribe(() => this.autoConvert());
    this.form.get('sourceCapitalId')!.valueChanges.subscribe(() => this.autoConvert());
    this.form.get('destinationCapitalId')!.valueChanges.subscribe(() => this.autoConvert());
  }

  get selectedSource(): CapitalResponse | null {
    const id = this.form.get('sourceCapitalId')?.value;
    return id ? this.capitals.find(c => c.id === Number(id)) ?? null : null;
  }

  get selectedDest(): CapitalResponse | null {
    const id = this.form.get('destinationCapitalId')?.value;
    return id ? this.capitals.find(c => c.id === Number(id)) ?? null : null;
  }

  get isCrossCurrency(): boolean {
    return !!this.selectedSource && !!this.selectedDest && this.selectedSource.currency !== this.selectedDest.currency;
  }

  private autoConvert(): void {
    const amount = this.form.get('amount')?.value;
    if (!amount || amount <= 0 || !this.selectedSource || !this.selectedDest) return;

    if (!this.isCrossCurrency) {
      this.form.get('destinationAmount')!.setValue(amount, { emitEvent: false });
    } else {
      const converted = this.convert(amount, this.selectedSource.currency, this.selectedDest.currency);
      this.form.get('destinationAmount')!.setValue(Math.round(converted * 100) / 100, { emitEvent: false });
    }
  }

  private convert(amount: number, from: string, to: string): number {
    const direct = this.exchanges.find(e => e.nationalCurrency === to && e.targetCurrency === from);
    if (direct) return amount * direct.sale;
    const reverse = this.exchanges.find(e => e.nationalCurrency === from && e.targetCurrency === to);
    if (reverse && reverse.buy > 0) return amount / reverse.buy;
    return amount;
  }

  submit(): void {
    if (this.form.invalid) return;
    const v = this.form.value;
    if (Number(v.sourceCapitalId) === Number(v.destinationCapitalId)) return;
    this.dialogRef.close({
      sourceCapitalId: Number(v.sourceCapitalId),
      destinationCapitalId: Number(v.destinationCapitalId),
      amount: Number(v.amount),
      destinationAmount: Number(v.destinationAmount)
    });
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
