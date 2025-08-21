import { Component, EventEmitter, OnDestroy, OnInit, Output } from '@angular/core';
import { CapitalService } from '../../services/capital.service';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { UpdateCapitalRequest } from '../../models/update-capital-request';
import { CurrencyType } from '../../../../core/types/currency-type';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Exchange } from '../../../../core/models/exchange-model';
import { DialogService } from '../../../../shared/services/dialog.service';
import { ConfirmDialogComponent } from '../../../../shared/components/dialogs/confirm-dialog/confirm-dialog.component';
import { CapitalResponse } from '../../models/capital-response';
import { FormField } from '../../../../shared/components/forms/models/form-field';
import { stringToCurrencyEnum } from '../../../../shared/components/currency/functions/string-to-currency-enum';

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

  formModified: boolean = false;

  unsubscribe = new Subject<void>;

  constructor(
    private readonly router: Router,
    private readonly capitalService: CapitalService,
    private readonly popupMessageService: PopupMessageService,
    private readonly dialogService: DialogService) {}

  ngOnInit(): void {
    this.initForm();
    this.initFields();

    this.form.valueChanges.subscribe(() => {
      this.formModified = !this.isFormEqualToModel();
    });

    this.form.get("currency")?.valueChanges.subscribe((value) => {
      const exchange = this.exchanges.find(x => x.targetCurrency === this.capital?.currency); // TODO && nationcurrency == value.currency

      if (value !== this.capital?.currency &&
          exchange
      ) {
        // TODO get exchanges and perform calculation, the best approach to separate logic of calc into separate service
        const saleAmount = (this.capital?.balance ?? 0) * exchange.sale;

        this.form.patchValue({ balance: saleAmount });
      }
      else if (this.capital?.currency !== this.form.get("balance")?.value){
        this.form.patchValue({ balance: this.capital?.balance });
      }
    })
  }

  ngOnDestroy(): void {
    this.unsubscribe.next();
    this.unsubscribe.complete();
  }

  initForm(): void {
    this.form = new FormGroup({
      Name: new FormControl(this.capital.name, [Validators.required, Validators.maxLength(32)]),
      Balance: new FormControl(this.capital.balance, Validators.required),
      Currency: new FormControl(stringToCurrencyEnum(this.capital.currency), Validators.required),
      IncludeInTotal: new FormControl(this.capital.includeInTotal, Validators.required)
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

  redirectToExpensePage(capitalId: number): void {
    this.router.navigate(['/expenses'], { queryParams: { capitalId: capitalId }});
    // TODO complete
  }

  saveChanges(): void {
    if (this.form.invalid || !this.capital) {
      this.popupMessageService.error('The capital was empty.');
      return;
    }

    const updatedCapital = this.form.value;

    const request: UpdateCapitalRequest = {
      id: this.capital.id,
      name: this.capital.name == updatedCapital.name ? null : updatedCapital.name,
      balance: this.capital.balance == updatedCapital.name ? null : updatedCapital.balance,
      currency: this.capital.currency == updatedCapital.currency ? null : updatedCapital.currency,
      includeInTotal: this.capital.includeInTotal === updatedCapital.includeInTotal ? null : updatedCapital.includeInTotal,
    };

    this.submitted.emit(request);
    this.dialogService.open({
              component: ConfirmDialogComponent,
              data: {
                title: 'update capital',
                action: 'update'
              },
              onSubmit: (confirmed: boolean) => {
                if (confirmed) {
                  this.capitalService
                    .update(request.id, request)
                    .pipe(takeUntil(this.unsubscribe))
                    .subscribe({
                      next: () => {
                        this.popupMessageService.success('The capital was successfully updated.');
                        this.formModified = false;
                        this.dialogService.close();
                      }
                    });
                } else {
                  this.cancelChanges();
                }
              }
            });
  }

  cancelChanges(): void {
    this.submitted.emit(null);

    this.form.reset(this.capital);
    this.formModified = false;
    this.dialogService.close();
  }

  remove(id: number): void {
    this.dialogService.open({
      component: ConfirmDialogComponent,
      data: {
        title: 'delete capital',
        action: 'delete'
      },
      onSubmit: (confirmed: boolean) => {
        if (confirmed) {
          this.capitalService
            .delete(id)
            .pipe(takeUntil(this.unsubscribe))
            .subscribe({
              next: () => {
                this.popupMessageService.success('The capital was successfully deleted.');
                this.router.navigate(['/capitals']);
                this.dialogService.close();
              }
            });
        } else {
          this.dialogService.close();
        }
      }
    });
  }

  isFormEqualToModel(): boolean {
    const capitalForm = this.form.value;

    return (
      capitalForm.name == this.capital?.name &&
      capitalForm.balance == this.capital?.balance &&
      capitalForm.currency == this.capital?.currency
    );
  }

  modelInvalidAndTouched(controlName: string): boolean | undefined {
    return this.form.get(controlName)?.invalid &&
          (this.form.get(controlName)?.dirty ||
          this.form.get(controlName)?.touched);
  }

  modelError(controlName: string, error: string): boolean {
    return (this.form.get(controlName)?.hasError(error) && this.form.get(controlName)?.touched)!;
  }
}
