import { Component, EventEmitter, OnDestroy, OnInit, Output } from '@angular/core';
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

@Component({
  selector: 'app-capital-details',
  templateUrl: './capital-details.component.html',
  styleUrl: './capital-details.component.scss'
})
export class CapitalDetailsComponent implements OnInit, OnDestroy {
  @Output() submitted = new EventEmitter<UpdateCapitalRequest | null>();

  capital: CapitalResponse;
  currencyOptions: { key: string, value: CurrencyType }[] = [];
  exchanges: Exchange[] = [];

  form: FormGroup;
  fields: FormField[] = [];
  buttons: FormButton[] = [];
  isFormModified: boolean = false;

  private unsubcribe$ = new Subject<void>;

  constructor(
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
      Name: new FormControl(this.capital.name, [Validators.required, Validators.maxLength(32)]),
      Balance: new FormControl(this.capital.balance, Validators.required),
      Currency: new FormControl(stringToCurrencyEnum(this.capital.currency), Validators.required),
      IncludeInTotal: new FormControl(this.capital.includeInTotal, Validators.required)
    });
  }

  checkFormModified(): void {
    const capitalCurrency = stringToCurrencyEnum(this.capital.currency);
    const capitalBalance = Number(this.capital.balance);

    this.form.valueChanges
      .pipe(takeUntil(this.unsubcribe$))
      .subscribe({
        next: (formValue) => {
          const isFormEqualToModel = (
            formValue.Name == this.capital.name &&
            formValue.Balance == capitalBalance &&
            formValue.Currency == capitalCurrency &&
            formValue.IncludeInTotal == this.capital.includeInTotal
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
          options: this.currencyOptions
        }
      },
      {
        label: 'Include in total',
        controlName: 'IncludeInTotal',
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

    let capitalCurrency = stringToCurrencyEnum(this.capital.currency);
    let updatedCurrency = !isNaN(updatedCapital.Currency) ? Number(updatedCapital.Currency) : capitalCurrency;

    const request: UpdateCapitalRequest = {
      id: this.capital.id,
      name: this.capital.name === updatedCapital.Name ? null : updatedCapital.Name,
      balance: this.capital.balance === updatedCapital.Balance ? null : updatedCapital.Balance,
      currency: capitalCurrency === updatedCurrency ? null : updatedCurrency,
      includeInTotal: this.capital.includeInTotal === updatedCapital.IncludeInTotal ? null : updatedCapital.IncludeInTotal,
    };

    this.submitted.emit(request);
  }

  handleCancel(): void {
    this.submitted.emit(null);
  }
}
