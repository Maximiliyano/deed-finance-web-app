import { ExpenseResponse } from "./expense-response";

export interface ExpenseCategoryResponse {
  categoryId: number;
  name: string;
  categorySum: number;
  percentage: number;
  plannedPeriodAmount: number;
  periodType: string;
  expenses: ExpenseResponse[];
}
