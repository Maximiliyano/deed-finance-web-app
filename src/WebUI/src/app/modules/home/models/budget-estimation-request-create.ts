export interface CreateBudgetEstimationRequest {
  description: string;
  budgetAmount: number;
  budgetCurrency: string;
  capitalId: number | null;
}
