import {Component, EventEmitter, OnInit, Output} from '@angular/core';
import {FormControl, FormGroup, Validators} from "@angular/forms";
import { CreateExpenseRequest } from '../../models/create-expense-request';
import { FormField } from '../../../../shared/components/forms/models/form-field';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SelectOptionModel } from '../../../../shared/components/forms/models/select-option-model';
import { noFutureDate } from '../../../../shared/components/forms/validators/noFutureDate';

@Component({
  selector: 'app-expense-dialog',
  templateUrl: './expense-dialog.component.html',
  styleUrl: './expense-dialog.component.scss'
})
export class ExpenseDialogComponent implements OnInit {
  @Output() submitted = new EventEmitter<CreateExpenseRequest | null>();

  categoryOptions: SelectOptionModel[] = [];
  capitalsOptions: SelectOptionModel[] = [];

  form: FormGroup;
  fields: FormField[] = [];

  constructor(private readonly popupMessageService: PopupMessageService) {}

  ngOnInit(): void {
    this.initForm();
    this.initFields();
  }

  private initFields(): void {
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
    ]
  }

  private initForm(): void {
    this.form = new FormGroup({
      Amount: new FormControl(0, [Validators.required, Validators.min(1)]),
      CapitalId: new FormControl({ value: '', disabled: this.capitalsOptions.length === 0 }, [Validators.required]),
      CategoryId: new FormControl({ value: '', disabled: this.categoryOptions.length === 0 }, [Validators.required]),
      PaymentDate: new FormControl(new Date().toISOString().split('T')[0], [Validators.required, noFutureDate]), // TODO move into function
      Purpose: new FormControl(null, [Validators.minLength(1)])
    });
  }

  handleSubmit(): void {
    if (this.form.invalid) {
      this.popupMessageService.error(`Invalid form data cannot be saved.`);
      return;
    }

    const capitalId = Number(this.form.value.CapitalId);
    const request: CreateExpenseRequest = {
      capitalId: capitalId,
      categoryId: Number(this.form.value.CategoryId),
      amount: Number(this.form.value.Amount),
      paymentDate: this.form.value.PaymentDate,
      purpose: this.form.value.Purpose
    };

    this.submitted.next(request);
  }

  handleCancel(): void {
    this.submitted.next(null);
  }
}
