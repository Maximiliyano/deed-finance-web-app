import { Exchange } from '../../core/models/exchange-model';

export function convertCurrency(
  amount: number,
  from: string,
  to: string,
  exchanges: Exchange[]
): number {
  if (!from || !to || from === to) return amount;

  const direct = exchanges.find(e => e.nationalCurrency === to && e.targetCurrency === from);
  if (direct) return amount * direct.sale;

  const reverse = exchanges.find(e => e.nationalCurrency === from && e.targetCurrency === to);
  if (reverse && reverse.buy > 0) return amount / reverse.buy;

  const base = exchanges[0]?.nationalCurrency;
  if (base && from !== base && to !== base) {
    const fromBase = exchanges.find(e => e.nationalCurrency === base && e.targetCurrency === from);
    const toBase = exchanges.find(e => e.nationalCurrency === base && e.targetCurrency === to);
    if (fromBase && toBase && toBase.buy > 0) {
      return (amount * fromBase.sale) / toBase.buy;
    }
  }

  return amount;
}
