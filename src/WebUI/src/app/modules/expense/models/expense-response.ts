import { Tag } from "./tag";

export interface ExpenseResponse {
  id: number;
  capitalId: number;
  amount: number;
  paymentDate: Date;
  purpose: string | null;
  tagNames: string[] | null;
}
