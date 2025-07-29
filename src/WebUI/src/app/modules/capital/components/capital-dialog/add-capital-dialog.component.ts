import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { CapitalService } from '../../services/capital.service';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { CurrencyType } from '../../../../core/types/currency-type';
import { AddCapitalRequest } from '../../models/add-capital-request';
import { FormFields } from '../../../../shared/components/forms/models/form-fields';
import { getEnumKeys } from '../../../../core/utils/enum-utils';
import { DialogService } from '../../../../shared/services/dialog.service';

@Component({
  selector: 'app-add-capital-dialog',
  templateUrl: './add-capital-dialog.component.html'
})
export class AddCapitalDialogComponent implements OnInit, OnDestroy {
  form: FormGroup;
  fields: FormFields[];

  readonly currencyOptions = getEnumKeys(CurrencyType);

  private readonly unsubscribe$ = new Subject<void>();

  constructor(
    private readonly capitalService: CapitalService,
    private readonly popupMessageService: PopupMessageService,
    private readonly dialogService: DialogService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.initFields();
  }

  ngOnDestroy(): void {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }

  private initForm(): void {
    this.form = new FormGroup({
      Name: new FormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(24)]),
      Balance: new FormControl(0, [Validators.required, Validators.min(0)]),
      Currency: new FormControl(CurrencyType.None, [Validators.required])
    });
  }

  private initFields(): void {
    this.fields = [
      {
        label: 'Name',
        controlName: 'Name',
        input: { type: 'text', placeholder: 'Capital name' }
      },
      {
        label: 'Balance',
        controlName: 'Balance',
        input: { type: 'number', placeholder: 'Balance' }
      },
      {
        label: 'Currency',
        controlName: 'Currency',
        select: { options: this.currencyOptions }
      }
    ];
  }

  handleSubmit(): void {
    if (this.form.invalid) {
      this.popupMessageService.error('The capital form is invalid.');
      return;
    }

    const request: AddCapitalRequest = {
      name: this.form.value.Name,
      balance: this.form.value.Balance,
      currency: this.form.value.Currency
    };

    this.capitalService.create(request)
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe({
        next: () => {
          this.popupMessageService.success("Capital successfully added.");
          this.handleCancel();
        }
    });
  }

  handleCancel() {
    this.dialogService.close();
  }
}
