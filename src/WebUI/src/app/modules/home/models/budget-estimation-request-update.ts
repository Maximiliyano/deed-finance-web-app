export interface UpdateBudgetEstimationRequest {
  description: string;
  budgetAmount: number;
  budgetCurrency: string;
  capitalId: number | null;
  isCompleted: boolean;
}
