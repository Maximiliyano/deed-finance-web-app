import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CurrencyType } from '../../../core/types/currency-type';
import { Currency } from './models/currency';
import { getCurrencies } from './functions/get-currencies.component';
import { stringToCurrencyEnum } from './functions/string-to-currency-enum';

@Component({
  selector: 'app-currency',
  templateUrl: './currency.component.html',
  styleUrl: './currency.component.scss'
})
export class CurrencyComponent implements OnInit {
  @Input() currency: string = 'None';
  @Input() withBorder: boolean = true;

  @Output() currencyChange = new EventEmitter<string>();

  type: CurrencyType;
  currencyOptions: Currency[];

  ngOnInit(): void {
    this.type = stringToCurrencyEnum(this.currency) ?? CurrencyType.None;
    this.refreshOptions();
  }

  onCurrencyChange(selectedType: CurrencyType): void {
    if (this.type !== selectedType) {
      const selectedCurrency = CurrencyType[selectedType];

      this.type = selectedType;
      this.currency = selectedCurrency;

      this.currencyChange.emit(selectedCurrency);

      this.refreshOptions();
    }
  }

  refreshOptions(): void {
    this.currencyOptions = getCurrencies(this.currency);
  }
}
