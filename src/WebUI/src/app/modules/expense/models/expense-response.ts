export interface ExpenseResponse {
  id: number;
  amount: number;
  paymentDate: Date;
  purpose: string | null;
}
