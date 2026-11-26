export type BankProvider = 'PrivatAutoclient' | 'Monobank';

export type BankImportStatus = 'None' | 'Pending' | 'Imported' | 'Dismissed';

export type BankTransactionType = 'None' | 'TopUp' | 'Withdrawal' | 'Transfer' | 'Payment';

export interface BankConnection {
  id: number;
  provider: BankProvider;
  label: string;
  status: 'Active' | 'TokenInvalid';
  maskedAccount: string;
  lastSyncAt: string;
  capitalId: number | null;
  autoImport: boolean;
  expenseCategoryId: number | null;
  incomeCategoryId: number | null;
  backfillFrom: string | null;
  backfillCursor: string | null;
}

export interface BankHistoryImportRequest {
  from: string;
  to: string | null;
}

export interface BankHistoryImportResponse {
  imported: number;
  inProgress: boolean;
}

export interface ConnectPrivatRequest {
  token: string;
  iban: string;
  label: string;
}

export interface ConnectMonobankRequest {
  token: string;
  accountId: string;
  iban: string;
  label: string;
}

export interface MonobankAccount {
  id: string;
  iban: string;
  currency: string;
  maskedCard: string | null;
  type: string;
  balance: number;
}

export interface UpdateBankConnectionRequest {
  label: string | null;
  capitalId: number | null;
  autoImport: boolean;
  expenseCategoryId: number | null;
  incomeCategoryId: number | null;
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
  importStatus: BankImportStatus;
  bankConnectionId: number | null;
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
