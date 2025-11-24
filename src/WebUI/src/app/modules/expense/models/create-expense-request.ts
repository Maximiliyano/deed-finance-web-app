export interface CreateExpenseRequest {
  capitalId: number;
  categoryId: number;
  amount: number;
  paymentDate: Date;
  purpose: string | null;
}
