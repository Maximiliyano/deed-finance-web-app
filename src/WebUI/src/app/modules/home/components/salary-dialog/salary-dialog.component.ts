import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { CurrencyType } from '../../../../core/types/currency-type';

export interface SalaryDialogData {
  salary: number;
  currency: string;
}

@Component({
  selector: 'app-salary-dialog',
  templateUrl: './salary-dialog.component.html',
  styleUrl: './salary-dialog.component.scss',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class SalaryDialogComponent implements OnInit {
  form!: FormGroup;
  currencies = Object.keys(CurrencyType).filter(k => isNaN(Number(k)) && k !== 'None');

  constructor(
    private readonly fb: FormBuilder,
    private readonly dialogRef: DialogRef<any>,
    @Inject(DIALOG_DATA) private readonly data: SalaryDialogData
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      salary: [this.data.salary ?? 0, [Validators.required, Validators.min(0)]],
      currency: [this.data.currency ?? 'UAH', Validators.required]
    });
  }

  submit(): void {
    if (this.form.invalid) return;
    const val = this.form.value;
    this.dialogRef.close({
      salary: val.salary,
      currency: CurrencyType[val.currency as keyof typeof CurrencyType] as unknown as number
    });
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
