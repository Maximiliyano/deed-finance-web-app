export interface CreateDebtRequest {
  item: string;
  amount: number;
  currency: string;
  source: string;
  recipient: string;
  borrowedAt: string;
  deadlineAt: string | null;
  note: string | null;
  capitalId: number | null;
}
