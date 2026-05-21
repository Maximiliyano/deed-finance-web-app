import { Component, Inject } from '@angular/core';
import { Exchange } from '../../../../core/models/exchange-model';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DIALOG_DATA } from '../models/dialog-consts';
import { getCurrencies } from '../../currency/functions/get-currencies.component';
import { convertCurrency } from '../../../utils/currency-conversion.util';

@Component({
    selector: 'app-exchange-dialog',
    templateUrl: './exchange-dialog.component.html',
    styleUrl: './exchange-dialog.component.scss',
    imports: [DecimalPipe, FormsModule],
    standalone: true
})
export class ExchangeDialogComponent {
  amount = 1;
  fromCurrency: string;
  toCurrency: string;

  readonly currencies: string[] = getCurrencies({ excludeNone: true }).map(c => c.key);

  constructor(
    @Inject(DIALOG_DATA) public exchanges: Exchange[]
  ) {
    const base = this.exchanges[0]?.nationalCurrency ?? 'UAH';
    const first = this.exchanges[0]?.targetCurrency ?? 'USD';
    this.fromCurrency = first;
    this.toCurrency = base;
  }

  get result(): number {
    return convertCurrency(this.amount || 0, this.fromCurrency, this.toCurrency, this.exchanges);
  }

  swap(): void {
    [this.fromCurrency, this.toCurrency] = [this.toCurrency, this.fromCurrency];
  }
}
