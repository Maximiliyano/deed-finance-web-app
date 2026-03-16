import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { Goal } from '../../models/goal.model';
import { CurrencyType } from '../../../../core/types/currency-type';

export interface GoalDialogData {
  goal?: Goal;
}

@Component({
  selector: 'app-goal-dialog',
  templateUrl: './goal-dialog.component.html',
  styleUrl: './goal-dialog.component.scss',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class GoalDialogComponent implements OnInit {
  form!: FormGroup;
  currencies = Object.keys(CurrencyType).filter(k => isNaN(Number(k)) && k !== 'None');
  isEdit = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly dialogRef: DialogRef<any>,
    @Inject(DIALOG_DATA) private readonly data: GoalDialogData
  ) {}

  ngOnInit(): void {
    this.isEdit = !!this.data.goal;
    const g = this.data.goal;

    this.form = this.fb.group({
      title: [g?.title ?? '', [Validators.required, Validators.maxLength(64)]],
      targetAmount: [g?.targetAmount ?? 0, [Validators.required, Validators.min(0.01)]],
      currency: [g?.currency ?? 'UAH', Validators.required],
      currentAmount: [g?.currentAmount ?? 0, [Validators.required, Validators.min(0)]],
      deadline: [g?.deadline ? g.deadline.slice(0, 10) : ''],
      note: [g?.note ?? ''],
      isCompleted: [g?.isCompleted ?? false]
    });
  }

  get progress(): number {
    const target = this.form.get('targetAmount')?.value ?? 0;
    const current = this.form.get('currentAmount')?.value ?? 0;
    if (!target) return 0;
    return Math.min(100, Math.round((current / target) * 100));
  }

  currencyIndex(name: string): number {
    return CurrencyType[name as keyof typeof CurrencyType] as unknown as number;
  }

  submit(): void {
    if (this.form.invalid) return;
    const val = this.form.value;
    this.dialogRef.close({
      title: val.title,
      targetAmount: val.targetAmount,
      currency: this.currencyIndex(val.currency),
      currentAmount: val.currentAmount,
      deadline: val.deadline || null,
      note: val.note || null,
      isCompleted: val.isCompleted
    });
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
