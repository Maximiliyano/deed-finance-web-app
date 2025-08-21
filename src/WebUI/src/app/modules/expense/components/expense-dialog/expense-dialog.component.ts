import {Component, EventEmitter, OnDestroy, OnInit, Output} from '@angular/core';
import {FormControl, FormGroup, Validators} from "@angular/forms";
import {Subject} from "rxjs";
import { DialogService } from '../../../../shared/services/dialog.service';
import {CapitalResponse} from "../../../capital/models/capital-response";
import {CategoryResponse} from "../../../../core/models/category-model";
import { AddExpenseRequest } from '../../models/add-expense-request';
import { FormField } from '../../../../shared/components/forms/models/form-field';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SelectOptionModel } from '../../../../shared/components/forms/models/select-option-model';

@Component({
  selector: 'app-expense-dialog',
  templateUrl: './expense-dialog.component.html',
  styleUrl: './expense-dialog.component.scss'
})
export class ExpenseDialogComponent implements OnInit, OnDestroy {
  @Output() submitted = new EventEmitter<AddExpenseRequest>();

  capitals: CapitalResponse[] = [];
  categories: CategoryResponse[] = [];

  categoryOptions: SelectOptionModel[] = [];
  capitalsOptions: SelectOptionModel[] = [];

  form: FormGroup;
  fields: FormField[] = [];

  private $unsubscribe = new Subject<void>();

  constructor(
    private readonly popupMessageService: PopupMessageService,
    private readonly dialogService: DialogService) {}

  ngOnInit(): void {
    this.initForm();
    this.initFields();
  }

  ngOnDestroy(): void {
    this.$unsubscribe.next();
    this.$unsubscribe.complete();
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
        dateTimePicker: {}
      },
      {
        label: 'Purpose',
        controlName: 'Purpose',
        textArea: {}
      }
    ]
  }

  private initForm(): void {
    this.categoryOptions = this.categories.map(x => { return { key: x.name, value: x.id } });
    this.capitalsOptions = this.capitals.map(x => { return { key: x.name, value: x.id } });

    this.form = new FormGroup({
      Amount: new FormControl('', [Validators.required]),
      CapitalId: new FormControl({ value: '', disabled: this.capitalsOptions.length === 0 }, Validators.required),
      CategoryId: new FormControl({ value: '', disabled: this.categoryOptions.length === 0 }, Validators.required),
      PaymentDate: new FormControl(new Date(), [Validators.required]),
      Purpose: new FormControl(null)
    });
  }

  handleSubmit(): void {
    if (!this.capitals.length || !this.categories.length) {
      this.popupMessageService.error(`The capitals or categories was not found.`);
      return;
    }

    const request: AddExpenseRequest = {
      capitalId: this.form.value.CapitalId,
      categoryId: this.form.value.CategoryId,
      amount: this.form.value.Amount,
      paymentDate: this.form.value.PaymentDate,
      purpose: this.form.value.Purpose
    };

    this.submitted.next(request);
  }

  handleCancel(): void {
    this.dialogService.close();
  }
}
