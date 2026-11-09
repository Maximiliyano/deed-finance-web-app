import { DestroyRef, Injectable, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { EMPTY, Observable, catchError, debounceTime, forkJoin, merge, of, switchMap } from 'rxjs';
import { AuthService } from '../../auth/services/auth-service';
import { CapitalService } from '../../capital/services/capital.service';
import { CURRENCY_DEFAULT } from '../../../core/types/currency-type';
import { ExchangeService } from '../../../shared/services/exchange.service';
import { DashboardSummary, DashboardSummaryParams } from '../models/dashboard-summary.model';
import { QueryTimeRange } from '../models/query-time-range';
import { BudgetEstimationService } from './budget-estimation.service';
import { DashboardService } from './dashboard.service';
import { UserSettingsService } from './user-settings.service';

export type PlanFilter = 'day' | 'month' | 'year';

@Injectable({ providedIn: 'root' })
export class DashboardStore {
  private readonly dashboardService = inject(DashboardService);
  private readonly authService = inject(AuthService);
  private readonly userSettingsService = inject(UserSettingsService);
  private readonly capitalService = inject(CapitalService);
  private readonly budgetEstimationService = inject(BudgetEstimationService);
  private readonly exchangeService = inject(ExchangeService);
  private readonly destroyRef = inject(DestroyRef);

  readonly planFilter = signal<PlanFilter>('month');
  readonly selectedDate = signal(new Date());
  readonly selectedCapitalIds = signal<ReadonlySet<number>>(new Set<number>());
  readonly summary = signal<DashboardSummary | null>(null);
  readonly chartSummary = signal<DashboardSummary | null>(null);

  readonly user = toSignal(this.authService.user$, { initialValue: null });
  readonly settings = toSignal(this.userSettingsService.settings$, { initialValue: null });
  readonly capitals = toSignal(this.capitalService.capitals$, { initialValue: [] });
  readonly exchanges = toSignal(this.exchangeService.getLatest(), { initialValue: [] });

  readonly isAnonymous = computed(() => !this.user());
  readonly userCurrency = computed(() => this.settings()?.currency ?? this.user()?.currency ?? CURRENCY_DEFAULT);
  readonly showDualCurrency = computed(() => this.userCurrency() !== CURRENCY_DEFAULT);
  readonly hasCapitalFilter = computed(() => this.selectedCapitalIds().size > 0);

  readonly chartCurrency = computed(() => {
    const ids = this.selectedCapitalIds();

    if (ids.size === 1) {
      const [id] = ids;
      const currency = this.capitals().find(c => c.id === id)?.currency;

      if (currency) {
        return currency;
      }
    }

    return this.userCurrency();
  });

  readonly period = computed<QueryTimeRange>(() => {
    const date = this.selectedDate();
    const year = date.getFullYear();
    const month = date.getMonth();

    switch (this.planFilter()) {
      case 'day':
        return { periodStart: new Date(year, month, date.getDate()), periodEnd: new Date(year, month, date.getDate() + 1) };
      case 'month':
        return { periodStart: new Date(year, month, 1), periodEnd: new Date(year, month + 1, 1) };
      case 'year':
        return { periodStart: new Date(year, 0, 1), periodEnd: new Date(year + 1, 0, 1) };
    }
  });

  readonly planFactor = computed(() => {
    switch (this.planFilter()) {
      case 'day': return 1 / 30;
      case 'year': return 12;
      default: return 1;
    }
  });

  readonly planFilterLabel = computed(() => {
    const filter = this.planFilter();
    return filter === 'day' ? 'Daily' : filter === 'year' ? 'Yearly' : 'Monthly';
  });

  readonly loss = computed(() => this.summary()?.totalBudget ?? 0);
  readonly lossInDefaultCurrency = computed(() => this.summary()?.totalBudgetInDefaultCurrency ?? 0);
  readonly totalCapital = computed(() => this.summary()?.totalCapital ?? 0);
  readonly totalIncome = computed(() => this.summary()?.totalIncome ?? 0);
  readonly totalIncomeInDefaultCurrency = computed(() => this.summary()?.totalIncomeInDefaultCurrency ?? 0);
  readonly profit = computed(() => this.totalIncome() - this.loss());
  readonly profitInDefaultCurrency = computed(() => this.totalIncomeInDefaultCurrency() - this.lossInDefaultCurrency());

  readonly filteredIncome = computed(() => this.chartSummary()?.totalIncome ?? 0);
  readonly filteredLoss = computed(() => this.chartSummary()?.totalBudget ?? 0);
  readonly filteredNetProfit = computed(() => this.filteredIncome() - this.filteredLoss());

  constructor() {
    merge(
      toObservable(this.period),
      toObservable(this.selectedCapitalIds),
      toObservable(this.userCurrency),
      this.capitalService.capitals$,
      this.budgetEstimationService.estimations$
    )
      .pipe(
        debounceTime(100),
        switchMap(() => this.loadSummaries()),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(([summary, chartSummary]) => {
        this.summary.set(summary);
        this.chartSummary.set(chartSummary ?? summary);
      });
  }

  setPlanFilter(filter: PlanFilter): void {
    this.planFilter.set(filter);
  }

  setDate(date: Date): void {
    this.selectedDate.set(date);
  }

  shiftPeriod(direction: 1 | -1): void {
    const next = new Date(this.selectedDate());

    switch (this.planFilter()) {
      case 'day':
        next.setDate(next.getDate() + direction);
        break;
      case 'month':
        next.setMonth(next.getMonth() + direction);
        break;
      case 'year':
        next.setFullYear(next.getFullYear() + direction);
        break;
    }

    this.selectedDate.set(next);
  }

  toggleCapital(id: number): void {
    const next = new Set(this.selectedCapitalIds());

    if (!next.delete(id)) {
      next.add(id);
    }

    this.selectedCapitalIds.set(next);
  }

  clearCapitals(): void {
    this.selectedCapitalIds.set(new Set<number>());
  }

  isCapitalSelected(id: number): boolean {
    return this.selectedCapitalIds().has(id);
  }

  private loadSummaries(): Observable<[DashboardSummary, DashboardSummary | null]> {
    const { periodStart, periodEnd } = this.period();
    const base: DashboardSummaryParams = { periodStart, periodEnd };

    const unfiltered$ = this.dashboardService.load({ ...base, currency: this.userCurrency() });
    const filtered$ = this.hasCapitalFilter()
      ? this.dashboardService.load({ ...base, currency: this.chartCurrency(), capitalIds: [...this.selectedCapitalIds()] })
      : of(null);

    return forkJoin([unfiltered$, filtered$]).pipe(catchError(() => EMPTY));
  }
}
