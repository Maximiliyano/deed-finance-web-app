export interface BudgetEstimation {
  id: number;
  description: string;
  budgetAmount: number;
  budgetCurrency: string;
  capitalId: number | null;
  capitalName: string | null;
  capitalBalance: number;
  capitalTotalExpense: number;
  capitalCurrency: string | null;
  orderIndex: number;
}
