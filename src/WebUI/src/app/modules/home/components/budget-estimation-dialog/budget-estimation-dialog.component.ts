import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { BudgetEstimation } from '../../models/budget-estimation.model';
import { CapitalResponse } from '../../../capital/models/capital-response';
import { CurrencyType } from '../../../../core/types/currency-type';

export interface BudgetEstimationDialogData {
  estimation?: BudgetEstimation;
  capitals: CapitalResponse[];
  mode?: 'edit' | 'copy';
}

@Component({
  selector: 'app-budget-estimation-dialog',
  templateUrl: './budget-estimation-dialog.component.html',
  styleUrl: './budget-estimation-dialog.component.scss',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class BudgetEstimationDialogComponent implements OnInit {
  form!: FormGroup;
  capitals: CapitalResponse[] = [];
  currencies = Object.keys(CurrencyType).filter(k => isNaN(Number(k)) && k !== 'None');
  isEdit = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly dialogRef: DialogRef<{ description: string; budgetAmount: number; budgetCurrency: number; capitalId: number | null; isCompleted: boolean } | null>,
    @Inject(DIALOG_DATA) private readonly data: BudgetEstimationDialogData
  ) {}

  ngOnInit(): void {
    this.capitals = this.data.capitals ?? [];
    const isCopy = this.data.mode === 'copy';
    this.isEdit = !isCopy && !!this.data.estimation;

    const src = this.data.estimation;
    const description = isCopy && src ? this.appendCopySuffix(src.description) : (src?.description ?? '');

    this.form = this.fb.group({
      description: [description, [Validators.required, Validators.maxLength(64)]],
      budgetAmount: [src?.budgetAmount ?? 0, [Validators.required, Validators.min(0)]],
      budgetCurrency: [src?.budgetCurrency ?? 'UAH', Validators.required],
      capitalId: [src?.capitalId ?? ''],
      isCompleted: [isCopy ? false : (src?.isCompleted ?? false)]
    });
  }

  private appendCopySuffix(description: string): string {
    const suffix = ' (copy)';
    const max = 64;
    return (description + suffix).length > max
      ? description.slice(0, max - suffix.length) + suffix
      : description + suffix;
  }

  get selectedCapital(): CapitalResponse | null {
    const id = Number(this.form.get('capitalId')?.value);
    return this.capitals.find(c => c.id === id) ?? null;
  }

  get totalSpent(): number {
    return this.selectedCapital?.totalExpense ?? 0;
  }

  get remains(): number {
    const budget = this.form.get('budgetAmount')?.value ?? 0;
    return budget - this.totalSpent;
  }

  currencyIndex(name: string): number {
    return CurrencyType[name as keyof typeof CurrencyType] as unknown as number;
  }

  submit(): void {
    if (this.form.invalid) return;
    const val = this.form.value;
    this.dialogRef.close({
      description: val.description,
      budgetAmount: val.budgetAmount,
      budgetCurrency: this.currencyIndex(val.budgetCurrency),
      capitalId: val.capitalId ? Number(val.capitalId) : null,
      isCompleted: val.isCompleted
    });
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
