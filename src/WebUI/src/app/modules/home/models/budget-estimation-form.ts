import { CapitalResponse } from "../../capital/models/capital-response";

export interface BudgetEstimationForm {
  description: string;
  budgetAmount: number;
  budgetCurrency: number;
  capitalId: number | null;
  capital: CapitalResponse | null;
  isCompleted: boolean;
}
