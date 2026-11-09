export interface CategoryAmount {
  categoryId: number;
  name: string;
  amount: number;
  percentage: number;
}

export interface DashboardSummary {
  currency: string;
  totalCapital: number;
  totalCapitalInDefaultCurrency: number;
  totalIncome: number;
  totalIncomeInDefaultCurrency: number;
  totalExpense: number;
  totalExpenseInDefaultCurrency: number;
  totalBudget: number;
  totalBudgetInDefaultCurrency: number;
  incomesByCategory: CategoryAmount[];
  expensesByCategory: CategoryAmount[];
}

export interface DashboardSummaryParams {
  currency?: string | null;
  capitalIds?: number[] | null;
  periodStart?: Date | null;
  periodEnd?: Date | null;
}
