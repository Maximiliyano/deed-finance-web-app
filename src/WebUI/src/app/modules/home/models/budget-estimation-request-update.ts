import { CurrencyType } from "../../../core/types/currency-type";

export interface UpdateBudgetEstimationRequest {
  description: string;
  budgetAmount: number;
  budgetCurrency: CurrencyType;
  capitalId: number | null;
  isCompleted: boolean;
}
