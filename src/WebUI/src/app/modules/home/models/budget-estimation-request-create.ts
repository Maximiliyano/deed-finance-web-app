import { CurrencyType } from "../../../core/types/currency-type";

export interface CreateBudgetEstimationRequest {
  description: string;
  budgetAmount: number;
  budgetCurrency: CurrencyType;
  capitalId: number | null;
}
