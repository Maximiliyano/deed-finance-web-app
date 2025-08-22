import { Pipe, PipeTransform } from "@angular/core";
import { CurrencySymbols } from "../models/currency-symbols";

@Pipe({ name: 'currencySymbol' })
export class CurrencySymbolPipe implements PipeTransform {
  transform(
    value: number,
    currencyCode: string,
    digitsInfo: string = '1.2-2',
    locale: string = 'en-US'
): string {
    if (value == null) return '';

    const symbol = CurrencySymbols[currencyCode] ?? currencyCode;

    const formatted = new Intl.NumberFormat(locale, {
      minimumFractionDigits: parseInt(digitsInfo.split('.')[1]?.split('-')[0] ?? '2', 10),
      maximumFractionDigits: parseInt(digitsInfo.split('-')[1] ?? '2', 10),
    }).format(value);

    return `${symbol} ${formatted}`;
  }
}
