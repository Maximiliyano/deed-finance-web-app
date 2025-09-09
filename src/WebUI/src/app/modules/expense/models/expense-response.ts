export interface ExpenseResponse {
  id: number;
  capitalId: number;
  amount: number;
  paymentDate: Date;
  purpose: string | null;
}
