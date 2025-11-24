import { Component, EventEmitter, Inject, OnDestroy, OnInit, Output } from '@angular/core';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { UpdateCapitalRequest } from '../../models/update-capital-request';
import { CurrencyType } from '../../../../core/types/currency-type';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Exchange } from '../../../../core/models/exchange-model';
import { CapitalResponse } from '../../models/capital-response';
import { FormField } from '../../../../shared/components/forms/models/form-field';
import { stringToCurrencyEnum } from '../../../../shared/components/currency/functions/string-to-currency-enum';
import { FormButton } from '../../../../shared/components/forms/models/form-button';
import { Subject, takeUntil } from 'rxjs';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';

@Component({
    selector: 'app-capital-details',
    templateUrl: './capital-details.component.html',
    styleUrl: './capital-details.component.scss',
    standalone: false
})
export class CapitalDetailsComponent implements OnInit, OnDestroy {
  form: FormGroup;
  fields: FormField[] = [];
  buttons: FormButton[] = [];
  isFormModified: boolean = false;

  private unsubcribe$ = new Subject<void>();

  constructor(
    @Inject(DIALOG_DATA) public data: {
      capital: CapitalResponse;
      currencyOptions: { key: string, value: CurrencyType }[];
      exchanges: Exchange[];
    },
    private readonly dialogRef: DialogRef<UpdateCapitalRequest | null>,
    private readonly popupMessageService: PopupMessageService) {}

  ngOnInit(): void {
    this.initForm();
    this.initFields();
    this.initButtons();
    this.checkFormModified();
  }

  ngOnDestroy(): void {
    this.unsubcribe$.next();
    this.unsubcribe$.complete();
  }

  initForm(): void {
    this.form = new FormGroup({
      Name: new FormControl(this.data.capital.name, [Validators.required, Validators.maxLength(32)]),
      Balance: new FormControl(this.data.capital.balance, Validators.required),
      Currency: new FormControl(stringToCurrencyEnum(this.data.capital.currency), Validators.required),
      IncludeInTotal: new FormControl(this.data.capital.includeInTotal, Validators.required),
      OnlyForSavings: new FormControl(this.data.capital.onlyForSavings)
    });
  }

  checkFormModified(): void {
    const capitalCurrency = stringToCurrencyEnum(this.data.capital.currency);
    const capitalBalance = Number(this.data.capital.balance);

    this.form.valueChanges
      .pipe(takeUntil(this.unsubcribe$))
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
          options: this.data.currencyOptions
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
        styles: `bg-[#377e21] ${!isSubmitBtnDisabled ? 'hover:bg-[#47a42b]' : 'cursor-not-allowed'} text-white`,
        disabled: isSubmitBtnDisabled,
      },
      {
        type: 'button',
        text: 'Cancel',
        styles: 'bg-[#e15240] hover:bg-[#f26243] text-white',
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

    const capitalCurrency = stringToCurrencyEnum(this.data.capital.currency);
    const updatedCurrency = !isNaN(updatedCapital.Currency) ? Number(updatedCapital.Currency) : capitalCurrency;

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
