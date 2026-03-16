export interface Debt {
  id: number;
  item: string;
  amount: number;
  currency: string;
  source: string;
  recipient: string;
  borrowedAt: string;
  deadlineAt: string | null;
  note: string | null;
  isPaid: boolean;
  capitalId: number | null;
  capitalName: string | null;
  orderIndex: number;
}
