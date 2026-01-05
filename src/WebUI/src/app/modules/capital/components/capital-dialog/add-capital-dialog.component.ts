import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { CurrencyType } from '../../../../core/types/currency-type';
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
    imports: [SharedModule, FormComponent]
})
export class AddCapitalDialogComponent implements OnInit, OnDestroy {
  form: FormGroup;
  fields: FormField[] = [];

  private readonly unsubscribe$ = new Subject<void>();

  constructor(
    @Inject(DIALOG_DATA) public currencyOptions: SelectOptionModel[],
    private readonly dialogRef: DialogRef<AddCapitalRequest | null>,
    private readonly popupMessageService: PopupMessageService
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
      Currency: new FormControl(CurrencyType.UAH, [Validators.required]),
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
      currency: Number(this.form.value.Currency),
      includeInTotal: this.form.value.IncludeInTotal,
      onlyForSavings: this.form.value.OnlyForSavings
    };

    this.dialogRef.close(request);
  }

  handleCancel(): void {
    this.dialogRef.close(null);
  }
}
