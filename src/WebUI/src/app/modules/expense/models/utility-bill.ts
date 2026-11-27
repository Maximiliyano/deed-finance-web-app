export interface UtilityBillResponse {
  id: number;
  name: string;
  estimatedAmount: number;
  currency: string;
  dueDayOfMonth: number;
  capitalId: number;
  capitalName: string | null;
  capitalCurrency: string | null;
  categoryId: number;
  categoryName: string | null;
  isActive: boolean;
  isPaidThisMonth: boolean;
  paidAmount: number | null;
  paidAt: string | null;
  createdAt: string;
  unitPrice: number | null;
  unit: string | null;
  previousReading: number | null;
  paidMeterReading: number | null;
}

export interface CreateUtilityBillRequest {
  name: string;
  estimatedAmount: number;
  currency: number;
  dueDayOfMonth: number;
  capitalId: number;
  categoryId: number;
  isActive: boolean;
  unitPrice?: number | null;
  unit?: string | null;
}

export interface UpdateUtilityBillRequest extends CreateUtilityBillRequest {}

export interface PayUtilityBillRequest {
  amount: number;
  paymentDate: string | null;
  meterReading?: number | null;
}
