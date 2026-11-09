import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { FormComponent } from "../../../../shared/components/forms/form.component";
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { FormField } from '../../../../shared/components/forms/models/form-field';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { SelectOptionModel } from '../../../../shared/components/forms/models/select-option-model';
import { CreateIncomeRequest } from '../../models/create-income-request';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';

@Component({
  selector: 'app-create-income-dialog.component',
  imports: [FormComponent],
  templateUrl: './create-income-dialog.component.html',
  styleUrl: './create-income-dialog.component.scss',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateIncomeDialogComponent implements OnInit {
  readonly data = inject<{ capitalsOptions: SelectOptionModel[]; categoryOptions: SelectOptionModel[]; }>(DIALOG_DATA);
  private readonly dialogRef = inject<DialogRef<CreateIncomeRequest | null>>(DialogRef);
  private readonly popupMessageService = inject(PopupMessageService);

  form: FormGroup;
  fields: FormField[];

  ngOnInit(): void {
    this.initFields();
    this.initForm();
  }

  handleSubmit(): void {
    if (this.form.invalid) {
      this.popupMessageService.error(`Invalid form data cannot be saved.`);
      return;
    }

    const capitalId = Number(this.form.value.CapitalId);
    const request: CreateIncomeRequest = {
      capitalId: capitalId,
      categoryId: Number(this.form.value.CategoryId),
      amount: Number(this.form.value.Amount),
      paymentDate: this.form.value.PaymentDate,
      purpose: this.form.value.Purpose,
    };

    this.dialogRef.close(request);
  }

  handleCancel(): void {
    this.dialogRef.close(null);
  }

  private initForm(): void {
    this.form = new FormGroup({
      amount: new FormControl(null, [Validators.required, Validators.min(0.01)]),
      paymentDate: new FormControl(null, Validators.required),
      categoryId: new FormControl({ value: '', disabled: this.data.categoryOptions.length === 0 }, Validators.required),
      capitalId: new FormControl({ value: '', disabled: this.data.capitalsOptions.length === 0 }, Validators.required),
      purpose: new FormControl(null, Validators.minLength(1)),
    });
  }

  private initFields(): void {
    this.fields = [
      {
        label: 'Amount',
        controlName: 'amount',
        input: { type: 'number' }
      },
      {
        label: 'Capital',
        controlName: 'capitalId',
        select: {
          options: this.data.capitalsOptions,
          optionCaption: 'Select a capital...'
        }
      },
      {
        label: 'Category',
        controlName: 'categoryId',
        select: {
          options: this.data.categoryOptions,
          optionCaption: 'Select a category...'
        }
      },
      {
        label: 'Payment date',
        controlName: 'paymentDate',
        dateTimePicker: {
          restrictFuture: true
        }
      },
      {
        label: 'Purpose',
        controlName: 'purpose',
        textArea: {}
      }
    ]
  }
}
