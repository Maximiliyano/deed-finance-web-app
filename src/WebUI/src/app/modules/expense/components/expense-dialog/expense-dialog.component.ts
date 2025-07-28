import {Component, Inject, OnDestroy, OnInit} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import {FormControl, FormGroup, Validators} from "@angular/forms";
import {Subject, takeUntil} from "rxjs";
import { ExpenseService } from '../../services/expense.service';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { DialogService } from '../../../../shared/services/dialog.service';
import {CapitalResponse} from "../../../capital/models/capital-response";
import {CategoryResponse} from "../../../../core/models/category-model";
import { AddExpenseRequest } from '../../models/add-expense-request';
import { ExpenseResponse } from '../../models/expense-response';
import { CategoryType } from '../../../../core/types/category-type';

@Component({
  selector: 'app-expense-dialog',
  templateUrl: './expense-dialog.component.html',
  styleUrl: './expense-dialog.component.scss'
})
export class ExpenseDialogComponent implements OnInit, OnDestroy {
  addExpenseForm: FormGroup;
  errCapitalMsg: string | null;
  errCategoryMsg: string | null;

  private $unsubscribe = new Subject<void>();

  constructor(
    private readonly dialogService: DialogService,
    private readonly expenseService: ExpenseService,
    private readonly popupService: PopupMessageService,
    public dialogRef: MatDialogRef<ExpenseDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { capitals: CapitalResponse[]; categories: CategoryResponse[] }) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  ngOnDestroy(): void {
    this.$unsubscribe.next();
    this.$unsubscribe.complete();
  }

  initializeForm(): void {
    this.addExpenseForm = new FormGroup({
      Amount: new FormControl('', [Validators.required]),
      CapitalId: new FormControl('', [Validators.required]),
      CategoryId: new FormControl('', [Validators.required]),
      PaymentDate: new FormControl(new Date(), [Validators.required]),
      Purpose: new FormControl(null)
    });

    this.checkAndToggleControl('CapitalId', !this.data.capitals?.length);

    this.checkAndToggleControl('CategoryId', !this.data.categories?.length);
  }

  checkAndToggleControl(controlName: string, hasItems: boolean): void {
    const control = this.addExpenseForm.get(controlName)!;

    if (hasItems) {
      if (!control.disabled) {
        control.disable();
        control.setErrors({ required: true, message: 'The control is required.'});
      }

      if (controlName === 'CapitalId') {
        this.errCapitalMsg = `You don't have any capitals assigned to your account.`;
      } else if (controlName === 'CategoryId') {
        this.errCategoryMsg = `You don't have any categories assigned to your account.`;
      }

    } else {
      control.enable();

      if (controlName === 'CapitalId') {
        this.errCapitalMsg = null;
      } else if (controlName === 'CategoryId') {
        this.errCategoryMsg = null;
      }
    }
  }

  add(): void {
    if (!this.data.capitals.length || !this.data.categories.length) {
      this.popupService.error(`The capitals or categories was not found.`);
      return;
    }

    const addExpenseRequest: AddExpenseRequest = {
      capitalId: this.addExpenseForm.value.CapitalId,
      categoryId: this.addExpenseForm.value.CategoryId,
      amount: this.addExpenseForm.value.Amount,
      paymentDate: this.addExpenseForm.value.PaymentDate,
      purpose: this.addExpenseForm.value.Purpose
    };

    this.expenseService.add(addExpenseRequest)
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: (id) => {
          this.popupService.success('The expense is successfully added.');

          let response: ExpenseResponse = {
            id: id,
            amount: addExpenseRequest.amount,
            paymentDate: addExpenseRequest.paymentDate,
            capitalId: addExpenseRequest.capitalId,
            category: {
              id: this.addExpenseForm.value.CategoryId,
              name: '',
              type: CategoryType.Expenses,
              totalExpenses: 0,
              totalExpensesPercent: '0'
            },
            purpose: addExpenseRequest.purpose
          };

          this.onCancel(response);
        },
        error: () => this.onCancel(null)
      });
  }

  onCancel(result: ExpenseResponse | null): void {
    this.dialogRef.close(result);
  }

  hasError(controlName: string, error: string): boolean {
    return this.dialogService.hasError(this.addExpenseForm, controlName, error);
  }
}
