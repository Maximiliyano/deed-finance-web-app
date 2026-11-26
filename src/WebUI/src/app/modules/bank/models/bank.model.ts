export type BankProvider = 'PrivatAutoclient';

export type BankTransactionType = 'None' | 'TopUp' | 'Withdrawal' | 'Transfer' | 'Payment';

export interface BankConnection {
  id: number;
  provider: BankProvider;
  label: string;
  status: 'Active' | 'TokenInvalid';
  maskedAccount: string;
  lastSyncAt: string;
}

export interface ConnectPrivatRequest {
  token: string;
  iban: string;
  label: string;
}

export interface BankNotification {
  id: number;
  kind: 'Transaction' | 'ConnectionIssue';
  type: BankTransactionType;
  direction: 'None' | 'Incoming' | 'Outgoing';
  amount: number;
  currency: string | null;
  counterparty: string | null;
  purpose: string | null;
  accountLabel: string | null;
  occurredAt: string;
  isRead: boolean;
}

export interface BankNotifications {
  unreadCount: number;
  items: BankNotification[];
}

export interface BankTransactionsPage {
  total: number;
  page: number;
  pageSize: number;
  items: BankNotification[];
}

export const BANK_TYPE_LABELS: Record<BankTransactionType, string> = {
  None: '',
  TopUp: 'Top-up',
  Withdrawal: 'Withdrawal',
  Transfer: 'Transfer',
  Payment: 'Payment'
};
