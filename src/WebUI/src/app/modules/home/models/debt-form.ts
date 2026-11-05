export interface DebtForm {
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
  payFromCapitalId: number;
}
