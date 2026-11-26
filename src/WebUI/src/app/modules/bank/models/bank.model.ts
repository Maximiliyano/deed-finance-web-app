export type BankProvider = 'PrivatAutoclient' | 'Monobank';

export type BankImportStatus = 'None' | 'Pending' | 'Imported' | 'Dismissed';

export type BankTransactionType = 'None' | 'TopUp' | 'Withdrawal' | 'Transfer' | 'Payment' | 'Exchange';

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
  isExcluded: boolean;
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

export interface PrivatAccount {
  iban: string;
  currency: string;
  name: string | null;
  balance: number | null;
}

export interface BankAccount {
  id: number;
  provider: BankProvider;
  iban: string;
  maskedAccount: string;
  currency: string;
  name: string | null;
  balance: number | null;
  lastRefreshedAt: string;
  connectionId: number | null;
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
  Payment: 'Payment',
  Exchange: 'Exchange'
};

export type BankSummaryPeriod = 'day' | 'month' | 'year';

export interface BankSummaryPeriodRow {
  start: string;
  income: number;
  expense: number;
  count: number;
}

export interface BankSummaryType {
  direction: 'Incoming' | 'Outgoing';
  type: string;
  amount: number;
  count: number;
}

export interface BankSummaryAccount {
  connectionId: number | null;
  label: string;
  income: number;
  expense: number;
  count: number;
}

export interface BankSummaryCounterparty {
  name: string | null;
  amount: number;
  count: number;
}

export interface BankSummaryCurrency {
  currency: string;
  income: number;
  expense: number;
  count: number;
  periods: BankSummaryPeriodRow[];
  types: BankSummaryType[];
  accounts: BankSummaryAccount[];
  topIncome: BankSummaryCounterparty[];
  topExpense: BankSummaryCounterparty[];
}

export interface BankSummary {
  period: string;
  from: string;
  to: string;
  currencies: BankSummaryCurrency[];
}
