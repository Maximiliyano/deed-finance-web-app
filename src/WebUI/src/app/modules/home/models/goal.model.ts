export interface Goal {
  id: number;
  title: string;
  targetAmount: number;
  currency: string;
  currentAmount: number;
  deadline: string | null;
  note: string | null;
  isCompleted: boolean;
  orderIndex: number;
}
