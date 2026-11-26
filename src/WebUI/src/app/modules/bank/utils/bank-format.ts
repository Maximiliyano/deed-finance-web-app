import { CURRENCY_SYMBOL } from '../../../core/types/currency-type';
import { BANK_TYPE_LABELS, BankNotification, BankTransactionType } from '../models/bank.model';

const NUMBER_FORMAT = new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

export const BANK_TYPE_ICONS: Record<BankTransactionType, string> = {
  None: 'fa-circle-info',
  TopUp: 'fa-arrow-down',
  Withdrawal: 'fa-money-bill-wave',
  Transfer: 'fa-right-left',
  Payment: 'fa-credit-card',
  Exchange: 'fa-arrows-rotate'
};

export const formatMoney = (amount: number, currency: string): string => {
  const text = NUMBER_FORMAT.format(amount);
  const symbol = CURRENCY_SYMBOL[currency] ?? currency;

  return symbol ? `${symbol} ${text}` : text;
};

export const bankTypeLabel = (item: BankNotification): string =>
  BANK_TYPE_LABELS[item.type] || (item.direction === 'Incoming' ? 'Incoming' : 'Outgoing');

export const hasClockTime = (isoDate: string): boolean => {
  const date = new Date(isoDate);

  return date.getHours() !== 0 || date.getMinutes() !== 0;
};
