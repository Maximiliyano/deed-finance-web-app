import { CurrencyType } from "../../../../core/types/currency-type";
import { Currency } from "../models/currency";

export function getCurrencies(currency: string = 'None'): Currency[] {
  return Object.keys(CurrencyType)
      .filter((key: any) =>
        !isNaN(Number(key)) &&
        CurrencyType[key] != 'None' &&
        (currency != 'None' && currency != CurrencyType[key]))
      .map((key: any) => ({ key: key, value: CurrencyType[key] }));
}
