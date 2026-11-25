import { Goal } from '../../models/goal.model';

export interface GoalBudgetSummary {
  freeBudget: number;
  reservedForDeadlines: number;
  availableForOpenGoals: number;
  openGoalCount: number;
}

export interface GoalAllocation {
  kind: 'deadline' | 'allocated' | 'overdue' | 'no-budget';
  monthly: number;
  months: number;
  remaining: number;
  feasible: boolean;
}

export interface GoalTip {
  tone: 'success' | 'warning' | 'danger';
  icon: string;
  label: string;
  amount: number;
  unit: string;
  detail: string;
  note: string | null;
}

export type RemainingOf = (goal: Goal) => number;

export function monthsUntil(deadline: string, now = new Date()): number {
  const date = new Date(deadline);
  return Math.max(0, (date.getFullYear() - now.getFullYear()) * 12 + (date.getMonth() - now.getMonth()));
}

export function summarizeBudget(goals: Goal[], profit: number, remainingOf: RemainingOf): GoalBudgetSummary {
  const free = Math.max(0, profit);
  let reserved = 0;
  let openCount = 0;

  for (const goal of goals) {
    if (goal.isCompleted) continue;
    const remaining = remainingOf(goal);
    if (remaining <= 0) continue;

    if (goal.deadline) {
      const months = monthsUntil(goal.deadline);
      if (months > 0) reserved += Math.ceil(remaining / months);
    } else {
      openCount++;
    }
  }

  return {
    freeBudget: free,
    reservedForDeadlines: Math.min(free, reserved),
    availableForOpenGoals: Math.max(0, free - reserved),
    openGoalCount: openCount
  };
}

export function allocateGoal(goal: Goal, summary: GoalBudgetSummary, remainingOf: RemainingOf): GoalAllocation | null {
  if (goal.isCompleted || !goal.targetAmount) return null;

  const remaining = remainingOf(goal);
  if (remaining <= 0) return null;

  if (goal.deadline) {
    const months = monthsUntil(goal.deadline);
    if (months === 0) return { kind: 'overdue', monthly: Math.ceil(remaining), months: 0, remaining, feasible: false };

    const monthly = Math.ceil(remaining / months);
    const feasible = summary.freeBudget >= summary.reservedForDeadlines && summary.freeBudget >= monthly;
    return { kind: 'deadline', monthly, months, remaining, feasible };
  }

  if (summary.openGoalCount === 0 || summary.availableForOpenGoals <= 0) {
    return { kind: 'no-budget', monthly: 0, months: 0, remaining, feasible: false };
  }

  const share = summary.availableForOpenGoals / summary.openGoalCount;
  const months = share > 0 ? Math.ceil(remaining / share) : 0;
  return { kind: 'allocated', monthly: Math.round(share), months, remaining, feasible: true };
}

export function toTip(allocation: GoalAllocation): GoalTip {
  const months = `${allocation.months} month${allocation.months === 1 ? '' : 's'}`;

  switch (allocation.kind) {
    case 'overdue':
      return {
        tone: 'danger',
        icon: 'fa-triangle-exclamation',
        label: 'Deadline passed:',
        amount: allocation.monthly,
        unit: '',
        detail: 'still needed',
        note: null
      };
    case 'no-budget':
      return {
        tone: 'danger',
        icon: 'fa-circle-exclamation',
        label: 'No free budget to allocate.',
        amount: allocation.remaining,
        unit: '',
        detail: 'remaining',
        note: null
      };
    case 'deadline':
      return {
        tone: allocation.feasible ? 'success' : 'warning',
        icon: allocation.feasible ? 'fa-lightbulb' : 'fa-circle-info',
        label: 'Save',
        amount: allocation.monthly,
        unit: '/mo',
        detail: `to hit deadline, ${months} left`,
        note: allocation.feasible ? null : 'exceeds free budget'
      };
    default:
      return {
        tone: 'success',
        icon: 'fa-lightbulb',
        label: 'Allocate',
        amount: allocation.monthly,
        unit: '/mo',
        detail: `ETA ${months}`,
        note: null
      };
  }
}
