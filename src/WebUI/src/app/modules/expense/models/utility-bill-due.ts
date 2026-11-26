import { UtilityBillResponse } from './utility-bill';

export type BillStatus = 'overdue' | 'soon' | 'upcoming' | 'paid' | 'inactive';

export interface BillDue {
  status: BillStatus;
  dueDate: Date;
  daysLeft: number;
  relative: string;
}

export const DUE_SOON_DAYS = 3;

const MS_PER_DAY = 86_400_000;

function startOfDay(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate());
}

function dueDateIn(year: number, month: number, dueDay: number): Date {
  const lastDay = new Date(year, month + 1, 0).getDate();
  return new Date(year, month, Math.min(dueDay, lastDay));
}

function relativeText(daysLeft: number): string {
  if (daysLeft === 0) return 'today';
  if (daysLeft === 1) return 'tomorrow';
  if (daysLeft > 1) return `in ${daysLeft} days`;
  return daysLeft === -1 ? '1 day late' : `${-daysLeft} days late`;
}

export function billDue(bill: UtilityBillResponse, now = new Date()): BillDue {
  const today = startOfDay(now);
  let dueDate = dueDateIn(today.getFullYear(), today.getMonth(), bill.dueDayOfMonth);

  if (bill.isPaidThisMonth) {
    dueDate = dueDateIn(today.getFullYear(), today.getMonth() + 1, bill.dueDayOfMonth);
  } else if (dueDate < today && startOfDay(new Date(bill.createdAt)) > dueDate) {
    dueDate = dueDateIn(today.getFullYear(), today.getMonth() + 1, bill.dueDayOfMonth);
  }

  const daysLeft = Math.round((dueDate.getTime() - today.getTime()) / MS_PER_DAY);
  const relative = relativeText(daysLeft);

  if (bill.isPaidThisMonth) return { status: 'paid', dueDate, daysLeft, relative };
  if (!bill.isActive) return { status: 'inactive', dueDate, daysLeft, relative };
  if (daysLeft < 0) return { status: 'overdue', dueDate, daysLeft, relative };
  if (daysLeft <= DUE_SOON_DAYS) return { status: 'soon', dueDate, daysLeft, relative };
  return { status: 'upcoming', dueDate, daysLeft, relative };
}
