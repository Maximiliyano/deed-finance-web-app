import { ChangeDetectionStrategy, Component, OnInit, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormGroup, FormControl, Validators } from '@angular/forms';

import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { CURRENCY_DEFAULT } from '../../../../core/types/currency-type';
import { AddCapitalRequest } from '../../models/add-capital-request';
import { FormField } from '../../../../shared/components/forms/models/form-field';
import { SelectOptionModel } from '../../../../shared/components/forms/models/select-option-model';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { SharedModule } from "../../../../shared/shared.module";
import { FormComponent } from '../../../../shared/components/forms/form.component';

@Component({
    selector: 'app-add-capital-dialog',
    templateUrl: './add-capital-dialog.component.html',
    standalone: true,
    imports: [SharedModule, FormComponent],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddCapitalDialogComponent implements OnInit {
  readonly currencyOptions = inject<SelectOptionModel[]>(DIALOG_DATA);
  private readonly dialogRef = inject<DialogRef<AddCapitalRequest | null>>(DialogRef);
  private readonly popupMessageService = inject(PopupMessageService);
  private readonly destroyRef = inject(DestroyRef);

  form: FormGroup;
  fields: FormField[] = [];

  ngOnInit(): void {
    this.initForm();
    this.initFields();
  }

  private initForm(): void {
    this.form = new FormGroup({
      Name: new FormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(24)]),
      Balance: new FormControl(0, [Validators.required, Validators.min(0)]),
      Currency: new FormControl(CURRENCY_DEFAULT, [Validators.required]),
      IncludeInTotal: new FormControl(true, [Validators.required]),
      OnlyForSavings: new FormControl(false)
    });
  }

  private initFields(): void {
    this.fields = [
      {
        label: 'Name',
        controlName: 'Name',
        input: { type: 'text', placeholder: 'Type a name...' }
      },
      {
        label: 'Balance',
        controlName: 'Balance',
        input: { type: 'number', placeholder: 'Type a balance...' }
      },
      {
        label: 'Currency',
        controlName: 'Currency',
        select: { options: this.currencyOptions }
      },
      {
        label: 'Include in Total balance',
        controlName: 'IncludeInTotal',
        input: { type: 'checkbox' }
      },
      {
        label: 'Only for savings',
        controlName: 'OnlyForSavings',
        input: { type: 'checkbox' }
      },
    ];
  }

  handleSubmit(): void {
    if (this.form.invalid) {
      this.popupMessageService.error('Invalid form data cannot be saved.');
      return;
    }

    const request: AddCapitalRequest = {
      name: this.form.value.Name,
      balance: Number(this.form.value.Balance),
      currency: this.form.value.Currency,
      includeInTotal: this.form.value.IncludeInTotal,
      onlyForSavings: this.form.value.OnlyForSavings
    };

    this.dialogRef.close(request);
  }

  handleCancel(): void {
    this.dialogRef.close(null);
  }
}
