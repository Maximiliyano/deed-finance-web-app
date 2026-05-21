export interface UtilityBillResponse {
  id: number;
  name: string;
  estimatedAmount: number;
  currency: string;
  dueDayOfMonth: number;
  capitalId: number;
  capitalName: string | null;
  categoryId: number;
  categoryName: string | null;
  isActive: boolean;
  isPaidThisMonth: boolean;
  paidAmount: number | null;
  paidAt: string | null;
  paidExpenseId: number | null;
}

export interface CreateUtilityBillRequest {
  name: string;
  estimatedAmount: number;
  currency: number;
  dueDayOfMonth: number;
  capitalId: number;
  categoryId: number;
  isActive: boolean;
}

export interface UpdateUtilityBillRequest extends CreateUtilityBillRequest {}

export interface PayUtilityBillRequest {
  amount: number;
  paymentDate: string | null;
}
