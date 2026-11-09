import { ChangeDetectorRef, ChangeDetectionStrategy, Component, OnInit, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { UpdateCapitalRequest } from '../../models/update-capital-request';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { CURRENCY_OPTIONS } from '../../../../core/types/currency-type';
import { CapitalResponse } from '../../models/capital-response';
import { FormField } from '../../../../shared/components/forms/models/form-field';
import { FormButton } from '../../../../shared/components/forms/models/form-button';

import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { FormComponent } from '../../../../shared/components/forms/form.component';

@Component({
    selector: 'app-capital-details',
    templateUrl: './capital-details.component.html',
    styleUrl: './capital-details.component.scss',
    imports: [FormComponent],
    standalone: true,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CapitalDetailsComponent implements OnInit {
  private readonly cdr = inject(ChangeDetectorRef);
  readonly data = inject<{ capital: CapitalResponse }>(DIALOG_DATA);
  private readonly dialogRef = inject<DialogRef<UpdateCapitalRequest | null>>(DialogRef);
  private readonly popupMessageService = inject(PopupMessageService);
  private readonly destroyRef = inject(DestroyRef);

  form: FormGroup;
  fields: FormField[] = [];
  buttons: FormButton[] = [];
  isFormModified: boolean = false;

  ngOnInit(): void {
    this.initForm();
    this.initFields();
    this.initButtons();
    this.checkFormModified();
  }

  initForm(): void {
    this.form = new FormGroup({
      Name: new FormControl(this.data.capital.name, [Validators.required, Validators.maxLength(32)]),
      Balance: new FormControl(this.data.capital.balance, Validators.required),
      Currency: new FormControl(this.data.capital.currency, Validators.required),
      IncludeInTotal: new FormControl(this.data.capital.includeInTotal, Validators.required),
      OnlyForSavings: new FormControl(this.data.capital.onlyForSavings)
    });
  }

  checkFormModified(): void {
    const capitalCurrency = this.data.capital.currency;
    const capitalBalance = Number(this.data.capital.balance);

    this.form.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (formValue) => {
          const isFormEqualToModel = (
            formValue.Name == this.data.capital.name &&
            formValue.Balance == capitalBalance &&
            formValue.Currency == capitalCurrency &&
            formValue.IncludeInTotal == this.data.capital.includeInTotal &&
            formValue.OnlyForSavings == this.data.capital.onlyForSavings
          );

          this.isFormModified = !isFormEqualToModel;

          this.initButtons();
          this.cdr.markForCheck();
        }
    });
  }

  initFields(): void {
    this.fields = [
      {
        label: 'Name',
        controlName: 'Name',
        input: {
          type: 'text'
        }
      },
      {
        label: 'Balance',
        controlName: 'Balance',
        input: {
          type: 'number'
        }
      },
      {
        label: 'Currency',
        controlName: 'Currency',
        select: {
          options: CURRENCY_OPTIONS
        }
      },
      {
        label: 'Include in total',
        controlName: 'IncludeInTotal',
        input: {
          type: 'checkbox'
        }
      },
      {
        label: 'Only For Savings',
        controlName: 'OnlyForSavings',
        input: {
          type: 'checkbox'
        }
      }
    ]
  }

  initButtons(): void {
    const isSubmitBtnDisabled = this.form.invalid || !this.isFormModified;

    this.buttons = [
      {
        type: 'submit',
        text: 'Update',
        styles: 'btn-primary',
        disabled: isSubmitBtnDisabled,
      },
      {
        type: 'button',
        text: 'Cancel',
        styles: 'btn-secondary',
        onClick: () => this.handleCancel()
      }
    ];
  }

  handleSubmit(): void {
    if (this.form.invalid || !this.isFormModified) {
      this.popupMessageService.error('Cannot update error occurred.');
      return;
    }

    const updatedCapital = this.form.value;

    const capitalCurrency = this.data.capital.currency;
    const updatedCurrency = updatedCapital.Currency;

    const request: UpdateCapitalRequest = {
      id: this.data.capital.id,
      name: this.data.capital.name === updatedCapital.Name ? null : updatedCapital.Name,
      balance: this.data.capital.balance === updatedCapital.Balance ? null : updatedCapital.Balance,
      currency: capitalCurrency === updatedCurrency ? null : updatedCurrency,
      includeInTotal: this.data.capital.includeInTotal === updatedCapital.IncludeInTotal ? null : updatedCapital.IncludeInTotal,
      onlyForSavings: this.data.capital.onlyForSavings === updatedCapital.OnlyForSavings ? null : updatedCapital.OnlyForSavings
    };

    this.dialogRef.close(request);
  }

  handleCancel(): void {
    this.dialogRef.close(null);
  }
}
