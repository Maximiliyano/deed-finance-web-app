import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, map } from 'rxjs';

export type SectionKey =
  | 'settings'
  | 'capitals'
  | 'estimations'
  | 'goals'
  | 'debts'
  | 'expenses'
  | 'incomes'
  | 'transfers'
  | 'exchanges'
  | 'categories';

const URL_TO_SECTION: [RegExp, SectionKey][] = [
  [/\/api\/user-settings/, 'settings'],
  [/\/api\/capitals/, 'capitals'],
  [/\/api\/budget-estimations/, 'estimations'],
  [/\/api\/goals/, 'goals'],
  [/\/api\/debts/, 'debts'],
  [/\/api\/expenses/, 'expenses'],
  [/\/api\/incomes/, 'incomes'],
  [/\/api\/transfers/, 'transfers'],
  [/\/api\/exchanges/, 'exchanges'],
  [/\/api\/categories/, 'categories'],
];

@Injectable({ providedIn: 'root' })
export class SectionLoadingService {
  private readonly counts = new Map<SectionKey, number>();
  private readonly state$ = new BehaviorSubject<ReadonlyMap<SectionKey, number>>(this.counts);

  resolve(url: string): SectionKey | null {
    for (const [pattern, key] of URL_TO_SECTION) {
      if (pattern.test(url)) return key;
    }
    return null;
  }

  start(key: SectionKey): void {
    this.counts.set(key, (this.counts.get(key) ?? 0) + 1);
    this.state$.next(this.counts);
  }

  stop(key: SectionKey): void {
    const c = (this.counts.get(key) ?? 1) - 1;
    if (c <= 0) this.counts.delete(key);
    else this.counts.set(key, c);
    this.state$.next(this.counts);
  }

  isLoading$(key: SectionKey): Observable<boolean> {
    return this.state$.pipe(map(m => (m.get(key) ?? 0) > 0));
  }

  isLoading(key: SectionKey): boolean {
    return (this.counts.get(key) ?? 0) > 0;
  }
}
