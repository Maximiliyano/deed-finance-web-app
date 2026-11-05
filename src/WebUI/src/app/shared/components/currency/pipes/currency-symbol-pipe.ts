import { Pipe, PipeTransform } from "@angular/core";
import { CURRENCY_SYMBOL } from "../../../../core/types/currency-type";

@Pipe({
    name: 'currencySymbol',
    standalone: false
})
export class CurrencySymbolPipe implements PipeTransform {
  transform(
    value: number,
    currencyCode: string,
    digits: string = '1.2-2',
    locale: string = 'en-US'
): string {
    if (value == null) return '';

    const [min, max] = digits.split('.')[1].split('-').map(Number);
    const num = new Intl.NumberFormat(locale, {
      minimumFractionDigits: min,
      maximumFractionDigits: max
    }).format(value);

    return `${CURRENCY_SYMBOL[currencyCode]} ${num}`;
  }
}
