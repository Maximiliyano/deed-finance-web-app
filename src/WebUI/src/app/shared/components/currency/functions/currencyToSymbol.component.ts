const currencySymbols: { [key: string]: string } = {
  PLN: 'zł',
  UAH: '₴',
};

export function currencyToSymbol(currency: string): string {
  return currencySymbols[currency] || currency;
}
