import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { merge } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { CapitalResponse } from '../../../capital/models/capital-response';
import { Exchange } from '../../../../core/models/exchange-model';

export interface TransferDialogData {
  capitals: CapitalResponse[];
  exchanges: Exchange[];
  sourceCapitalId?: number | null;
}

@Component({
  selector: 'app-transfer-dialog',
  templateUrl: './transfer-dialog.component.html',
  styleUrl: './transfer-dialog.component.scss',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TransferDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject<DialogRef<any>>(DialogRef);
  private readonly data = inject<TransferDialogData>(DIALOG_DATA);
  private readonly destroyRef = inject(DestroyRef);

  form!: FormGroup;
  capitals: CapitalResponse[] = this.data.capitals;
  exchanges: Exchange[] = this.data.exchanges ?? [];

  ngOnInit(): void {
    const presetSource = this.data.sourceCapitalId ?? null;
    this.form = this.fb.group({
      sourceCapitalId: [presetSource, Validators.required],
      destinationCapitalId: [null, Validators.required],
      amount: [null, [Validators.required, Validators.min(0.01)]],
      destinationAmount: [null, [Validators.required, Validators.min(0.01)]]
    });

    merge(
      this.form.controls['amount'].valueChanges,
      this.form.controls['sourceCapitalId'].valueChanges,
      this.form.controls['destinationCapitalId'].valueChanges
    )
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.autoConvert());
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
