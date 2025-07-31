import { CurrencyType } from "../../../../core/types/currency-type";
import { Currency } from "../models/currency";

export function getCurrencies(currency: string = 'None'): Currency[] {
  return Object.keys(CurrencyType)
      .filter((key: string) =>
        key !== 'None' &&
        (currency !== 'None' && currency !== key))
      .map((key: any) => ({ key: key, value: key }));
}
