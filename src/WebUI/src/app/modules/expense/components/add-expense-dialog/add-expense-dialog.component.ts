import {Component, Inject, OnInit} from '@angular/core';
import {FormControl, FormGroup, Validators} from "@angular/forms";
import { CreateExpenseRequest } from '../../models/create-expense-request';
import { FormField } from '../../../../shared/components/forms/models/form-field';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SelectOptionModel } from '../../../../shared/components/forms/models/select-option-model';
import { noFutureDate } from '../../../../shared/components/forms/validators/noFutureDate';
import { SharedModule } from "../../../../shared/shared.module";
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { FormComponent } from '../../../../shared/components/forms/form.component';
import { Tag } from '../../models/tag';

@Component({
    selector: 'app-add-expense-dialog',
    templateUrl: './add-expense-dialog.component.html',
    styleUrl: './add-expense-dialog.component.scss',
    standalone: true,
    imports: [SharedModule, FormComponent]
})
export class AddExpenseDialogComponent implements OnInit {
  form: FormGroup;
  fields: FormField[] = [];

  constructor(
    @Inject(DIALOG_DATA) public data: {
      categoryOptions: SelectOptionModel[];
      capitalsOptions: SelectOptionModel[];
      tags: Tag[];
    },
    private readonly dialogRef: DialogRef<CreateExpenseRequest | null>,
    private readonly popupMessageService: PopupMessageService) {}

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
          options: this.data.capitalsOptions,
          optionCaption: 'Select a capital...'
        }
      },
      {
        label: 'Category',
        controlName: 'CategoryId',
        select: {
          options: this.data.categoryOptions,
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
        label: 'Tags',
        controlName: 'TagNames',
        selectiveInput: {
          data: this.data.tags,
          onSearch: this.onSearch,
          onCreate: this.onCreate
        }
      },
      {
        label: 'Purpose',
        controlName: 'Purpose',
        textArea: {}
      }
    ]
  }

  onCreate(names: string[]): void {}
  onSearch(term?: string): void {}

  private initForm(): void {
    this.form = new FormGroup({
      Amount: new FormControl(null, [Validators.required, Validators.min(1)]),
      CapitalId: new FormControl({ value: '', disabled: this.data.capitalsOptions.length === 0 }, [Validators.required]),
      CategoryId: new FormControl({ value: '', disabled: this.data.categoryOptions.length === 0 }, [Validators.required]),
      TagNames: new FormControl([]),
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
    const tags = this.form.value.TagNames as string[] | [];
    const request: CreateExpenseRequest = {
      capitalId: capitalId,
      categoryId: Number(this.form.value.CategoryId),
      amount: Number(this.form.value.Amount),
      paymentDate: this.form.value.PaymentDate,
      purpose: this.form.value.Purpose,
      tagNames: tags
    };

    this.dialogRef.close(request);
  }

  handleCancel(): void {
    this.dialogRef.close(null);
  }
}
