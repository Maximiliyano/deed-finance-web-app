import { computed, signal } from '@angular/core';
import { QueryTimeRange } from './query-time-range';

export type PeriodFilter = 'day' | 'month' | 'year' | 'custom';

const DAY_MS = 86_400_000;

const startOfDay = (date: Date): Date => new Date(date.getFullYear(), date.getMonth(), date.getDate());
const addDays = (date: Date, days: number): Date => new Date(date.getFullYear(), date.getMonth(), date.getDate() + days);
const daysInMonth = (year: number, month: number): number => new Date(year, month + 1, 0).getDate();
const shiftMonths = (date: Date, months: number): Date => {
  const first = new Date(date.getFullYear(), date.getMonth() + months, 1);

  return new Date(first.getFullYear(), first.getMonth(), Math.min(date.getDate(), daysInMonth(first.getFullYear(), first.getMonth())));
};

export class PeriodState {
  readonly filter = signal<PeriodFilter>('month');
  readonly enabled = signal(true);
  readonly date = signal(new Date());
  readonly customFrom = signal(new Date(new Date().getFullYear(), new Date().getMonth(), 1));
  readonly customTo = signal(startOfDay(new Date()));

  readonly range = computed<QueryTimeRange>(() => {
    const date = this.date();
    const year = date.getFullYear();
    const month = date.getMonth();

    switch (this.filter()) {
      case 'day':
        return { periodStart: startOfDay(date), periodEnd: addDays(date, 1) };
      case 'month':
        return { periodStart: new Date(year, month, 1), periodEnd: new Date(year, month + 1, 1) };
      case 'year':
        return { periodStart: new Date(year, 0, 1), periodEnd: new Date(year + 1, 0, 1) };
      case 'custom':
        return { periodStart: startOfDay(this.customFrom()), periodEnd: addDays(this.customTo(), 1) };
    }
  });

  readonly customLabel = computed(() => {
    const from = this.customFrom();
    const to = this.customTo();
    const format = (date: Date, withYear: boolean): string =>
      date.toLocaleDateString('en-GB', { day: 'numeric', month: 'short', ...(withYear ? { year: 'numeric' } : {}) });

    return `${format(from, from.getFullYear() !== to.getFullYear())} - ${format(to, true)}`;
  });

  setFilter(filter: PeriodFilter): void {
    if (filter === 'custom' && this.filter() !== 'custom') {
      const { periodStart, periodEnd } = this.range();
      this.customFrom.set(periodStart);
      this.customTo.set(addDays(periodEnd, -1));
    }

    this.filter.set(filter);
    this.enabled.set(true);
  }

  setDate(date: Date): void {
    this.date.set(date);
  }

  setCustomFrom(from: Date): void {
    this.customFrom.set(from);

    if (this.customTo() < from) {
      this.customTo.set(from);
    }
  }

  setCustomTo(to: Date): void {
    this.customTo.set(to);

    if (this.customFrom() > to) {
      this.customFrom.set(to);
    }
  }

  shift(direction: 1 | -1): void {
    const current = this.date();

    switch (this.filter()) {
      case 'day':
        this.date.set(addDays(current, direction));
        break;
      case 'month':
        this.date.set(shiftMonths(current, direction));
        break;
      case 'year':
        this.date.set(shiftMonths(current, 12 * direction));
        break;
      case 'custom': {
        const span = Math.round((this.customTo().getTime() - this.customFrom().getTime()) / DAY_MS) + 1;
        this.customFrom.set(addDays(this.customFrom(), span * direction));
        this.customTo.set(addDays(this.customTo(), span * direction));
        break;
      }
    }
  }
}
