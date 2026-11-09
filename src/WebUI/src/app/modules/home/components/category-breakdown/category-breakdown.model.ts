export interface BreakdownItem {
  id: number;
  capitalId: number;
  categoryId: number;
  amount: number;
  purpose: string | null;
  paymentDate: Date;
}

export interface BreakdownGroup {
  key: string | number;
  name: string;
  count: number;
  total: number;
  percentage: number | null;
  items: BreakdownItem[];
}

export interface ReorderEvent {
  previousIndex: number;
  currentIndex: number;
}
