export interface Capital {
  id: number;
  name: string;
  balance: number;
  currency: string;
  includeInTotal: boolean;
  totalIncome: number;
  totalExpense: number;
  transferOut: number;
  transferIn: number;
  editable: boolean;
}
