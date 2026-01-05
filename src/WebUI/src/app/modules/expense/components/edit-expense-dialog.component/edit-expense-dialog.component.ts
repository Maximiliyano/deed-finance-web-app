import { Component, Inject, OnInit } from '@angular/core';
import { SharedModule } from "../../../../shared/shared.module";
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { FormField } from '../../../../shared/components/forms/models/form-field';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { UpdateExpenseRequest } from '../../models/update-expense.request';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { ExpenseResponse } from '../../models/expense-response';
import { SelectOptionModel } from '../../../../shared/components/forms/models/select-option-model';
import { noFutureDate } from '../../../../shared/components/forms/validators/noFutureDate';
import { inputDateToISO, toInputDateString } from '../../../../core/utils/date';
import { FormComponent } from "../../../../shared/components/forms/form.component";
import { Tag } from '../../models/tag';

@Component({
  selector: 'app-edit-expense-dialog.component',
  imports: [SharedModule, FormComponent],
  templateUrl: './edit-expense-dialog.component.html',
  styleUrl: './edit-expense-dialog.component.scss',
  standalone: true
})
export class EditExpenseDialogComponent implements OnInit { // TODO
  form: FormGroup;
  fields: FormField[] = [];

  private expense: ExpenseResponse;
  private categoryId: number;
  private capitalsOptions: SelectOptionModel[];
  private categoryOptions: SelectOptionModel[];

  constructor(
    @Inject(DIALOG_DATA) public data: {
      expense: ExpenseResponse,
      categoryId: number,
      capitalOptions: SelectOptionModel[],
      categoryOptions: SelectOptionModel[],
      tags: Tag[]
    }, 
    private readonly dialogRef: DialogRef<UpdateExpenseRequest | null>,
    private readonly popupMessageService: PopupMessageService
  ) {
    this.expense = data.expense;
    this.categoryId = data.categoryId;
    this.capitalsOptions = data.capitalOptions;
    this.categoryOptions = data.categoryOptions;
  }

  ngOnInit(): void {
    this.initForm();
    this.initFields();
  }

  initForm(): void {
    this.form = new FormGroup({
      Amount: new FormControl(this.expense.amount, [Validators.required, Validators.min(1)]),
      CapitalId: new FormControl(
        { value: this.expense.capitalId, disabled: this.capitalsOptions.length === 0 },
        [Validators.required]
      ),
      CategoryId: new FormControl(
        { value: this.categoryId, disabled: this.categoryOptions.length === 0 },
        [Validators.required]
      ),
      PaymentDate: new FormControl(
        toInputDateString(this.expense.paymentDate) ?? '',
        [Validators.required, noFutureDate]
      ),
      Purpose: new FormControl(this.expense.purpose, [Validators.minLength(1)]),
      TagNames: new FormControl(this.expense.tagNames)
    });
  }

  initFields(): void {
    this.fields = [
      {
        label: 'Amount',
        controlName: 'Amount',
        input: { type: 'number' }
      },
      {
        label: 'Capital',
        controlName: 'CapitalId',
        select: {
          options: this.capitalsOptions,
          optionCaption: 'Select a capital...'
        }
      },
      {
        label: 'Category',
        controlName: 'CategoryId',
        select: {
          options: this.categoryOptions,
          optionCaption: 'Select a category...'
        }
      },
      {
        label: 'Tags',
        controlName: 'TagNames',
        selectiveInput: {
          data: this.data.tags,
          onSearch: this.onSearch,
          onCreate: this.onCreate
        }
      },
      {
        label: 'Payment date',
        controlName: 'PaymentDate',
        dateTimePicker: {
          restrictFuture: true
        }
      },
      {
        label: 'Purpose',
        controlName: 'Purpose',
        textArea: {}
      }
    ];
  }

  onCreate(tags: string[]): void {}
  onSearch(term?: string): void {}

  handleSubmit(): void {
    if (this.form.invalid) {
      this.popupMessageService.error('Invalid form data, expense cannot be updated.');
      return;
    }

    const formValue = this.form.getRawValue();
    
    const formPaymentDateStr: string | null = formValue.PaymentDate
      ? formValue.PaymentDate.toString()
      : null;
    const expensePaymentDateStr = toInputDateString(this.expense.paymentDate);

    const request: UpdateExpenseRequest = {
      id: this.expense.id,

      capitalId:
        Number(formValue.CapitalId) !== this.expense.capitalId
          ? Number(formValue.CapitalId)
          : undefined,

      categoryId:
        Number(formValue.CategoryId) !== this.categoryId
          ? Number(formValue.CategoryId)
          : undefined,

      amount:
        Number(formValue.Amount) !== this.expense.amount
          ? Number(formValue.Amount)
          : undefined,

      date: formPaymentDateStr && formPaymentDateStr !== expensePaymentDateStr
        ? inputDateToISO(formPaymentDateStr)
        : undefined,

      tagNames:
        JSON.stringify(formValue.TagNames) !== JSON.stringify(this.expense.tagNames)
          ? formValue.TagNames
          : undefined,
          
      purpose: formValue.Purpose?.trim() === '' ? null : formValue.Purpose
    };

    this.dialogRef.close(request);
  }

  handleCancel(): void {
    this.dialogRef.close(null);
  }
}
