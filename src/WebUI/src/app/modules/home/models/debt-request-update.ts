export interface UpdateDebtRequest {
  item: string;
  amount: number;
  currency: string;
  source: string;
  recipient: string;
  borrowedAt: string;
  deadlineAt: string | null;
  note: string | null;
  isPaid: boolean;
  payFromCapitalId: number | null;
}
