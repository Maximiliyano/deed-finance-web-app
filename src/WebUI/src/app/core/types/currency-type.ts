export const CURRENCY_SYMBOL: Record<string, string> = {
  UAH: '₴',
  USD: '$',
  EUR: '€',
  PLN: 'zł',
  CZK: 'Kč'
};
export const CURRENCIES = [
  'UAH',
  'USD',
  'EUR',
  'PLN',
  'CZK'
] as const;
export const CURRENCY_DEFAULT: string = CURRENCIES[0];
export const CURRENCY_OPTIONS = CURRENCIES.map(currency => ({ key: currency, value: currency }));

export type Currency = typeof CURRENCIES[number];
