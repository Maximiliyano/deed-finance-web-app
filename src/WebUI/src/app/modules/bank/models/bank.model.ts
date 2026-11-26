export interface BankConnection {
  connected: boolean;
  provider: 'PrivatAutoclient' | 'PrivatCard' | null;
  status: 'Active' | 'TokenInvalid' | null;
  maskedAccount: string | null;
  lastSyncAt: string | null;
}

export interface ConnectPrivatRequest {
  token: string;
  iban: string;
}

export interface ConnectPrivatCardRequest {
  merchantId: string;
  password: string;
  cardNumber: string;
}

export interface BankNotification {
  id: number;
  kind: 'Transaction' | 'ConnectionIssue';
  direction: 'None' | 'Incoming' | 'Outgoing';
  amount: number;
  currency: string | null;
  counterparty: string | null;
  purpose: string | null;
  occurredAt: string;
  isRead: boolean;
}

export interface BankNotifications {
  unreadCount: number;
  items: BankNotification[];
}

export const EMPTY_BANK_CONNECTION: BankConnection = {
  connected: false,
  provider: null,
  status: null,
  maskedAccount: null,
  lastSyncAt: null
};
