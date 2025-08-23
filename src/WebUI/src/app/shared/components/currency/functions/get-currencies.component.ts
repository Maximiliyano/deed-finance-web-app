import { CurrencyType } from "../../../../core/types/currency-type";

export function getCurrencies(options?: { excludeNone?: boolean }): { key: string, value: CurrencyType }[] {
  const excludeNone = options?.excludeNone ?? false;

  return Object.keys(CurrencyType)
    .filter(k => isNaN(Number(k)))
    .filter(k => !(excludeNone && k === 'None'))
    .map(k => ({
      key: k,
      value: CurrencyType[k as keyof typeof CurrencyType]
    }));
}
