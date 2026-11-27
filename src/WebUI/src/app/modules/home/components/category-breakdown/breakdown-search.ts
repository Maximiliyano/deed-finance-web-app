import { computed, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { debounce, timer } from 'rxjs';
import { AmountSort } from '../../../../shared/components/sort-toolbar/sort-toolbar.component';
import { BreakdownGroup, BreakdownItem } from './category-breakdown.model';

const DEBOUNCE_MS = 200;

export function normalizeSearch(value: string): string {
  return value.normalize('NFD').replace(/\p{M}/gu, '').toLowerCase().trim();
}

function matchesItem(item: BreakdownItem, needle: string): boolean {
  return (
    (item.purpose !== null && normalizeSearch(item.purpose).includes(needle)) ||
    (item.tagNames ?? []).some(tag => normalizeSearch(tag).includes(needle))
  );
}

export function searchGroups(groups: BreakdownGroup[], term: string): BreakdownGroup[] {
  const needle = normalizeSearch(term);
  if (!needle) return groups;

  const matched: BreakdownGroup[] = [];

  for (const group of groups) {
    if (normalizeSearch(group.name).includes(needle)) {
      matched.push(group);
      continue;
    }

    const items = group.items.filter(item => matchesItem(item, needle));
    if (items.length === 0) continue;

    matched.push({ ...group, items, count: items.length, total: items.reduce((sum, item) => sum + item.amount, 0) });
  }

  const total = matched.reduce((sum, group) => sum + group.total, 0);

  return matched.map(group => ({
    ...group,
    percentage: total > 0 ? (group.total / total) * 100 : group.percentage === null ? null : 0
  }));
}

export function sortGroups(groups: BreakdownGroup[], by: AmountSort): BreakdownGroup[] {
  switch (by) {
    case 'amount-asc':
      return [...groups].sort((a, b) => a.total - b.total);
    case 'name':
      return [...groups].sort((a, b) => a.name.localeCompare(b.name));
    case 'custom':
      return groups;
    default:
      return [...groups].sort((a, b) => b.total - a.total);
  }
}

export class BreakdownSearch {
  readonly input = signal('');
  readonly term = toSignal(
    toObservable(this.input).pipe(debounce(value => timer(value.trim() === '' ? 0 : DEBOUNCE_MS))),
    { initialValue: '' }
  );
  readonly active = computed(() => this.term().trim() !== '');

  set(event: Event): void {
    this.input.set((event.target as HTMLInputElement).value);
  }

  clear(): void {
    this.input.set('');
  }
}
