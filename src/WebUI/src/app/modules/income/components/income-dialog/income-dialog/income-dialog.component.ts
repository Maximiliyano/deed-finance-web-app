import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';
import { IncomeService } from '../../../services/income.service';
import { IncomeType } from '../../../models/income-type';
import { AddIncomeRequest } from '../../../models/income';

@Component({
  selector: 'app-income-dialog',
  templateUrl: './income-dialog.component.html',
  styleUrl: './income-dialog.component.scss'
})
export class IncomeDialogComponent implements OnInit, OnDestroy {
  addIncomeForm: FormGroup;
  incomeType = IncomeType;

  private $unsubscribe = new Subject<void>;

  constructor(
    private readonly incomeService: IncomeService,
    private readonly dialogRef: MatDialogRef<IncomeDialogComponent>,
    @Inject(MAT_DIALOG_DATA) private readonly data: number) {
  }

  ngOnInit(): void {
    this.addIncomeForm = new FormGroup({
      Amount: new FormControl(0, [Validators.required]),
      Type: new FormControl(this.incomeType.Salary, [Validators.required]),
      Purpose: new FormControl('', [Validators.required])
    })
  }

  ngOnDestroy(): void {
    this.$unsubscribe.next();
    this.$unsubscribe.complete();
  }

  addNewIncome(): void {
    const request: AddIncomeRequest = {
      capitalId: this.data,
      amount: this.addIncomeForm.value.Amount,
      type: this.addIncomeForm.value.Type,
      purpose: this.addIncomeForm.value.Purpose
    };

    this.incomeService.add(request)
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: () => this.close(true),
        error: () => this.close(false)
      });
  }

  close(response: boolean): void {
    this.dialogRef.close(response);
  }
}
