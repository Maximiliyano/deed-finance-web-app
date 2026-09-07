import {ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit} from '@angular/core';
import {Subject, combineLatest, of, skip, startWith, switchMap, takeUntil} from 'rxjs';
import {ActivatedRoute} from '@angular/router';
import {moveItemInArray} from '@angular/cdk/drag-drop';
import {AuthService} from '../auth/services/auth-service';
import {User} from '../auth/models/user';
import {Debt} from './models/debt.model';
import {BudgetEstimation} from './models/budget-estimation.model';
import {Goal} from './models/goal.model';
import {UserSettings} from './models/user-settings.model';
import {
  BudgetEstimationService,
  CreateBudgetEstimationRequest,
  UpdateBudgetEstimationRequest
} from './services/budget-estimation.service';
import {GoalService, UpdateGoalRequest} from './services/goal.service';
import {CreateDebtRequest, DebtService, UpdateDebtRequest} from './services/debt.service';
import {UserSettingsService} from './services/user-settings.service';
import {DashboardLayoutService, PanelId} from './services/dashboard-layout.service';
import {DialogService} from '../../shared/components/dialogs/services/dialog.service';
import {CapitalService} from '../capital/services/capital.service';
import {CapitalResponse} from '../capital/models/capital-response';
import {ExpenseService} from '../expense/services/expense.service';
import {ExpenseCategoryResponse} from '../expense/models/expense-category-response';
import {IncomeService} from '../incomes/services/income.service';
import {IncomeResponse} from '../incomes/models/income-response';
import {CategoryService} from '../category/services/category.service';
import {CategoryResponse} from '../category/models/category-model';
import {
  BudgetEstimationDialogComponent,
  BudgetEstimationDialogData
} from './components/budget-estimation-dialog/budget-estimation-dialog.component';
import {GoalDialogComponent, GoalDialogData} from './components/goal-dialog/goal-dialog.component';
import {DebtDialogComponent, DebtDialogData} from './components/debt-dialog/debt-dialog.component';
import {PopupMessageService} from '../../shared/services/popup-message.service';
import {
  QuickTransactionDialogComponent
} from './components/quick-transaction-dialog/quick-transaction-dialog.component';
import {TransferResponse, TransferService} from '../../shared/services/transfer.service';
import {TransferDialogComponent, TransferDialogData} from './components/transfer-dialog/transfer-dialog.component';
import {Exchange} from '../../core/models/exchange-model';
import {CurrencyType} from '../../core/types/currency-type';
import {CapitalDetailsComponent} from '../capital/components/capital-details/capital-details.component';
import {AddCapitalDialogComponent} from '../capital/components/capital-dialog/add-capital-dialog.component';
import {AddCapitalRequest} from '../capital/models/add-capital-request';
import {getCurrencies} from '../../shared/components/currency/functions/get-currencies.component';
import {UpdateCapitalRequest} from '../capital/models/update-capital-request';
import {SectionKey, SectionLoadingService} from '../../shared/services/section-loading.service';
import {convertCurrency} from '../../shared/utils/currency-conversion.util';
import { NavItem } from '../../core/layout/header/header.component';
import { QueryTimeRange } from './models/query-time-range';
import { CategoryType } from '../../core/types/category-type';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrl: './home.component.scss',
    standalone: false,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent implements OnInit, OnDestroy {
  user: User | null = null;
  goals: Goal[] = [];
  debts: Debt[] = [];
  budgetEstimations: BudgetEstimation[] = [];
  capitals: CapitalResponse[] = [];
  transfers: TransferResponse[] = [];
  expenses: ExpenseCategoryResponse[] = [];
  incomes: IncomeResponse[] = [];
  incomeCategories: CategoryResponse[] = [];
  expenseCategoriesList: CategoryResponse[] = [];
  userSettings: UserSettings | null = null;
  exchanges: Exchange[] = [];
  isEditMode = false;
  budgetEstimationSortBy: 'custom' | 'amount-asc' | 'amount-desc' | 'name' = 'custom';
  capitalSortBy: 'custom' | 'balance-desc' | 'balance-asc' | 'name' = 'custom';
  goalSortBy: 'custom' | 'progress-desc' | 'progress-asc' | 'name' = 'custom';
  debtSortBy: 'custom' | 'amount-desc' | 'amount-asc' | 'name' = 'custom';
  expenseSortBy: 'custom' | 'default' | 'amount-desc' | 'amount-asc' | 'name' = 'default';
  incomeSortBy: 'custom' | 'default' | 'amount-desc' | 'amount-asc' | 'name' = 'default';
  planFilter: 'day' | 'month' | 'year' = 'month';
  showCharts = false;
  chartTab: 'general' | 'expenses' | 'incomes' = 'general';
  selectedCapitalIds = new Set<number>();
  selectedDate = new Date();

  customStartDate?: Date;
  customEndDate?: Date;

  private readonly reloadBudget$ = new Subject<void>();

  readonly barColors = ['#60a5fa', '#f472b6', '#34d399', '#fb923c', '#38bdf8', '#facc15', '#2dd4bf'];

  readonly capitalObservable = this.capitalService
    .load({ searchTerm: null, sortBy: null, sortDirection: null, filterBy: null });

  readonly budgetEstimationObservable = this.reloadBudget$.pipe(
    startWith(void 0),
    switchMap(() => this.budgetEstimationService
      .load(this.getBudgetEstimationRequest())
    )
  );

  readonly expenseObservable = this.expenseService
    .load();
  readonly incomeObservable = this.incomeService
    .load();
  readonly categoryObservable = this.categoryService.getAll();
  readonly goalObservable = this.goalService.getAll();
  readonly debtObservable = this.debtService.getAll();

  private unsubscribe$ = new Subject<void>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly authService: AuthService,
    private readonly budgetEstimationService: BudgetEstimationService,
    private readonly goalService: GoalService,
    private readonly debtService: DebtService,
    private readonly userSettingsService: UserSettingsService,
    private readonly capitalService: CapitalService,
    private readonly expenseService: ExpenseService,
    private readonly incomeService: IncomeService,
    private readonly categoryService: CategoryService,
    private readonly dialogService: DialogService,
    private readonly popup: PopupMessageService,
    private readonly transferService: TransferService,
    readonly layoutService: DashboardLayoutService,
    readonly sectionLoading: SectionLoadingService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    document.title = 'Deed - Home page';

    combineLatest([
      this.sectionLoading.isLoading$('settings'),
      this.sectionLoading.isLoading$('capitals'),
      this.sectionLoading.isLoading$('budgetEstimations'),
      this.sectionLoading.isLoading$('expenses'),
      this.sectionLoading.isLoading$('incomes'),
      this.sectionLoading.isLoading$('goals'),
      this.sectionLoading.isLoading$('debts'),
    ]).pipe(skip(1), takeUntil(this.unsubscribe$))
      .subscribe(() => this.cdr.markForCheck());

    this.authService.me()
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe({ next: user => { this.user = user; this.cdr.markForCheck() } });

    this.subscribeToStores();

    this.route.fragment.pipe(takeUntil(this.unsubscribe$)).subscribe(fragment => {
      if (fragment) {
        setTimeout(() => {
          const el = document.getElementById(fragment);
          if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 300);
      }
    });
  }

  get isAnonymous(): boolean {
    return !this.user;
  }

  get userCurrency(): string {
    return this.userSettings?.currency ?? 'UAH';
  }

  get loss(): number {
    return this.budgetEstimations.reduce(
      (sum, e) => sum + convertCurrency(e.budgetAmount, e.budgetCurrency, this.userCurrency, this.exchanges), 0
    );
  }

  get profit(): number {
    return this.totalIncomesConverted - this.loss;
  }

  get isProfitable(): boolean {
    return this.profit >= 0;
  }

  get lossPercent(): number {
    return this.totalCapitalAmount > 0
      ? Math.min(100, Math.round((this.loss / this.totalCapitalAmount) * 100))
      : 0;
  }

  get profitPercent(): number {
    return Math.max(0, 100 - this.lossPercent);
  }

  navItems: NavItem[] = [
    { label: 'Budget Planner', icon: 'fa-compass-drafting', link: '/', fragment: 'estimations' },
    { label: 'Capitals', icon: 'fa-wallet', link: '/capitals' },
    { label: 'Expenses', icon: 'fa-money-bill-wave', link: '/expenses' },
    { label: 'Incomes', icon: 'fa-dollar-sign', link: '/incomes' },
    { label: 'Goals', icon: 'fa-star', link: '/', fragment: 'goals' },
    { label: 'Debts', icon: 'fa-hand-holding-dollar', link: '/', fragment: 'debts' },
  ];

  budgetPercent(amount: number, fromCurrency?: string): number {
    if (this.totalCapitalAmount <= 0) return 0;
    const converted = fromCurrency ? convertCurrency(amount, fromCurrency, this.userCurrency, this.exchanges) : amount;
    return Math.min(100, Math.round((converted / this.totalCapitalAmount) * 100));
  }

  get showDualCurrency(): boolean {
    return this.userCurrency !== 'UAH';
  }

  get lossInUah(): number {
    return this.budgetEstimations.reduce(
      (sum, e) => sum + convertCurrency(e.budgetAmount, e.budgetCurrency, 'UAH', this.exchanges), 0
    );
  }

  get profitInUah(): number {
    return this.totalIncomesInUah - this.lossInUah;
  }

  get totalCapitalAmount(): number {
    return this.capitals
      .filter(c => c.includeInTotal)
      .reduce((sum, c) => sum + convertCurrency(c.balance, c.currency, this.userCurrency, this.exchanges), 0);
  }

  get totalCapitalAmountInUah(): number {
    return this.capitals
      .filter(c => c.includeInTotal)
      .reduce((sum, c) => sum + convertCurrency(c.balance, c.currency, CurrencyType.UAH.toString(), this.exchanges), 0);
  }

  private get planFactor(): number {
    switch (this.planFilter) {
      case 'day':  return 1 / 30;
      case 'year': return 12;
      default:     return 1;
    }
  }

  get scaledLoss(): number       { return this.loss      * this.planFactor; }
  get scaledProfit(): number     { return this.profit    * this.planFactor; }
  get scaledLossInUah(): number  { return this.lossInUah * this.planFactor; }
  get scaledProfitInUah(): number { return this.profitInUah * this.planFactor; }
  get scaledIncomes(): number    { return this.totalIncomesConverted * this.planFactor; }

  get lossCoverage(): number {
    if (this.totalIncomesConverted <= 0) return 0;
    return Math.min(100, Math.round((this.loss / this.totalIncomesConverted) * 100));
  }

  convertUserCurrency(amount: number, from: string): number {
    return convertCurrency(amount, from, this.userCurrency, this.exchanges);
  }

  toggleCharts(): void {
    this.showCharts = !this.showCharts;
    this.cdr.markForCheck();
  }

  setChartTab(tab: 'general' | 'expenses' | 'incomes'): void {
    this.chartTab = tab;
    this.cdr.markForCheck();
  }

  toggleCapitalFilter(id: number): void {
    if (this.selectedCapitalIds.has(id)) {
      this.selectedCapitalIds.delete(id);
    } else {
      this.selectedCapitalIds.add(id);
    }
    this.selectedCapitalIds = new Set(this.selectedCapitalIds);
    this.cdr.markForCheck();
  }

  clearCapitalFilter(): void {
    this.selectedCapitalIds = new Set<number>();
    this.cdr.markForCheck();
  }

  get hasCapitalFilter(): boolean {
    return this.selectedCapitalIds.size > 0;
  }

  isCapitalSelected(id: number): boolean {
    return this.selectedCapitalIds.has(id);
  }

  private capitalAllowed(id: number | null | undefined): boolean {
    if (!this.hasCapitalFilter) return true;
    return id != null && this.selectedCapitalIds.has(id);
  }

  get filteredIncomes(): IncomeResponse[] {
    if (!this.hasCapitalFilter) return this.incomes;
    return this.incomes.filter(i => this.capitalAllowed(i.capitalId));
  }

  get chartCurrency(): string {
    if (this.selectedCapitalIds.size === 1) {
      const id = [...this.selectedCapitalIds][0];
      const cap = this.capitals.find(c => c.id === id);
      if (cap?.currency) return cap.currency;
    }
    return this.userCurrency;
  }

  private capitalCurrencyOf(capitalId: number | null | undefined): string {
    if (capitalId == null) return this.chartCurrency;
    const cap = this.capitals.find(c => c.id === capitalId);
    return cap?.currency ?? this.chartCurrency;
  }

  get filteredTotalIncomes(): number {
    const target = this.chartCurrency;
    return this.filteredIncomes.reduce((s, i) =>
      s + convertCurrency(i.amount, this.capitalCurrencyOf(i.capitalId), target, this.exchanges), 0);
  }

  get filteredLoss(): number {
    const target = this.chartCurrency;
    return this.budgetEstimations
      .filter(e => this.capitalAllowed(e.capitalId))
      .reduce((sum, e) => sum + convertCurrency(e.budgetAmount, e.budgetCurrency, target, this.exchanges), 0);
  }

  get filteredNetProfit(): number {
    return this.filteredTotalIncomes - this.filteredLoss;
  }

  get scaledFilteredIncomes(): number { return this.filteredTotalIncomes * this.planFactor; }
  get scaledFilteredLoss(): number    { return this.filteredLoss          * this.planFactor; }
  get scaledFilteredNet(): number     { return this.filteredNetProfit     * this.planFactor; }

  get donutLossPct(): number {
    if (this.filteredTotalIncomes <= 0) return this.filteredLoss > 0 ? 100 : 0;
    return Math.min(100, Math.max(0, (this.filteredLoss / this.filteredTotalIncomes) * 100));
  }

  get donutNetPct(): number {
    return Math.max(0, 100 - this.donutLossPct);
  }

  get chartIncomesByCategory(): { name: string; amount: number; pct: number; color: string }[] {
    const target = this.chartCurrency;
    const byName = new Map<string, number>();
    for (const inc of this.filteredIncomes) {
      const cat = this.incomeCategories.find(c => c.id === inc.categoryId);
      const name = cat?.name ?? 'Unknown';
      const converted = convertCurrency(inc.amount, this.capitalCurrencyOf(inc.capitalId), target, this.exchanges);
      byName.set(name, (byName.get(name) ?? 0) + converted);
    }
    const total = [...byName.values()].reduce((s, v) => s + v, 0);
    if (total <= 0) return [];
    return [...byName.entries()]
      .map(([name, amount], i) => ({
        name, amount,
        pct: (amount / total) * 100,
        color: this.barColors[i % this.barColors.length]
      }))
      .sort((a, b) => b.amount - a.amount);
  }

  get chartExpensesByCategory(): { name: string; amount: number; pct: number; color: string }[] {
    const target = this.chartCurrency;
    const filtered = this.expenses.map(c => {
      const expenses = this.hasCapitalFilter
        ? c.expenses.filter((e: any) => this.capitalAllowed(e.capitalId))
        : c.expenses;
      const sum = expenses.reduce((s: number, e: any) =>
        s + convertCurrency(e.amount, this.capitalCurrencyOf(e.capitalId), target, this.exchanges), 0);
      return { name: c.name, sum };
    });
    const total = filtered.reduce((s, c) => s + c.sum, 0);
    if (total <= 0) return [];
    return filtered
      .filter(c => c.sum > 0)
      .map((c, i) => ({
        name: c.name,
        amount: c.sum,
        pct: (c.sum / total) * 100,
        color: this.barColors[i % this.barColors.length]
      }))
      .sort((a, b) => b.amount - a.amount);
  }

  get filteredTotalExpensesConverted(): number {
    const target = this.chartCurrency;
    let sum = 0;
    for (const cat of this.expenses) {
      const expenses = this.hasCapitalFilter
        ? cat.expenses.filter((e: any) => this.capitalAllowed(e.capitalId))
        : cat.expenses;
      for (const e of expenses) {
        sum += convertCurrency(e.amount, this.capitalCurrencyOf((e as any).capitalId), target, this.exchanges);
      }
    }
    return sum;
  }

  get planFilterLabel(): string {
    return this.planFilter === 'day' ? 'Daily' : this.planFilter === 'year' ? 'Yearly' : 'Monthly';
  }

  setPlanFilter(scope: 'day' | 'month' | 'year'): void {
    this.planFilter = scope;
    this.reloadBudget$.next();
    this.cdr.markForCheck();
  }

  get selectedDay(): string {
    const d = this.selectedDate;
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  get selectedMonth(): string {
    const d = this.selectedDate;
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
  }

  get selectedYear(): number {
    return this.selectedDate.getFullYear();
  }

  get selectedPeriodLabel(): string {
    const opts: Intl.DateTimeFormatOptions =
      this.planFilter === 'day'  ? { year: 'numeric', month: 'short', day: 'numeric' } :
      this.planFilter === 'year' ? { year: 'numeric' } :
                                   { year: 'numeric', month: 'long' };
    return this.selectedDate.toLocaleDateString(undefined, opts);
  }

  onDayChange(value: string): void {
    this.reloadBudget$.next();
    if (!value) return;
    const d = new Date(`${value}T00:00:00`);
    if (!isNaN(d.getTime())) {
      this.selectedDate = d;
      this.cdr.markForCheck();
    }
  }

  onMonthChange(value: string): void {
    this.reloadBudget$.next();
    if (!value) return;
    const [y, m] = value.split('-').map(Number);
    if (!Number.isFinite(y) || !Number.isFinite(m)) return;
    const d = new Date(this.selectedDate);
    d.setDate(1);
    d.setFullYear(y);
    d.setMonth(m - 1);
    this.selectedDate = d;
    this.cdr.markForCheck();
  }

  onYearChange(value: string | number): void {
    this.reloadBudget$.next();
    const y = typeof value === 'string' ? Number(value) : value;
    if (!Number.isFinite(y) || y < 1900 || y > 2100) return;
    const d = new Date(this.selectedDate);
    d.setFullYear(y);
    this.selectedDate = d;
    this.cdr.markForCheck();
  }

  setNextPeriod(): void {
    switch (this.planFilter) {
      case 'day':
        this.selectedDate.setDate(this.selectedDate.getDate() + 1);
        break;
      case 'month':
        this.selectedDate.setMonth(this.selectedDate.getMonth() + 1);
        break;
      case 'year':
        this.selectedDate.setFullYear(this.selectedDate.getFullYear() + 1);
        break;
    }

    this.reloadBudget$.next();
  }

  setPreviousPeriod(): void {
    switch (this.planFilter) {
      case 'day':
        this.selectedDate.setDate(this.selectedDate.getDate() - 1);
        break;
      case 'month':
        this.selectedDate.setMonth(this.selectedDate.getMonth() - 1);
        break;
      case 'year':
        this.selectedDate.setFullYear(this.selectedDate.getFullYear() - 1);
        break;
    }

    this.reloadBudget$.next();
  }

  goalProgress(goal: Goal): number {
    if (!goal.targetAmount) return 0;
    return Math.min(100, Math.round((goal.currentAmount / goal.targetAmount) * 100));
  }

  private monthsUntil(deadline: string): number {
    const now = new Date();
    const d = new Date(deadline);
    return Math.max(0, (d.getFullYear() - now.getFullYear()) * 12 + (d.getMonth() - now.getMonth()));
  }

  private goalRemaining(goal: Goal): number {
    return convertCurrency(goal.targetAmount - goal.currentAmount, goal.currency, this.userCurrency, this.exchanges);
  }

  get goalBudgetSummary(): { freeBudget: number; reservedForDeadlines: number; availableForOpenGoals: number; openGoalCount: number } {
    const free = Math.max(0, this.profit);
    let reserved = 0;
    let openCount = 0;
    for (const g of this.goals) {
      if (g.isCompleted) continue;
      const remaining = this.goalRemaining(g);
      if (remaining <= 0) continue;
      if (g.deadline) {
        const months = this.monthsUntil(g.deadline);
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

  goalAllocation(goal: Goal): { kind: 'deadline' | 'allocated' | 'overdue' | 'no-budget' | 'no-target'; monthly: number; months: number; remaining: number; feasible: boolean } | null {
    if (goal.isCompleted || !goal.targetAmount) return null;
    const remaining = this.goalRemaining(goal);
    if (remaining <= 0) return null;

    const summary = this.goalBudgetSummary;

    if (goal.deadline) {
      const months = this.monthsUntil(goal.deadline);
      if (months === 0) return { kind: 'overdue', monthly: Math.ceil(remaining), months: 0, remaining, feasible: false };
      const monthly = Math.ceil(remaining / months);
      return { kind: 'deadline', monthly, months, remaining, feasible: summary.freeBudget >= summary.reservedForDeadlines && summary.freeBudget >= monthly };
    }

    if (summary.openGoalCount === 0 || summary.availableForOpenGoals <= 0) {
      return { kind: 'no-budget', monthly: 0, months: 0, remaining, feasible: false };
    }
    const share = summary.availableForOpenGoals / summary.openGoalCount;
    const months = share > 0 ? Math.ceil(remaining / share) : 0;
    return { kind: 'allocated', monthly: Math.round(share), months, remaining, feasible: true };
  }

  debtStatus(debt: Debt): 'overdue' | 'soon' | 'ok' {
    if (!debt.deadlineAt) return 'ok';
    const diffDays = Math.ceil((new Date(debt.deadlineAt).getTime() - Date.now()) / 86400000);
    if (diffDays < 0) return 'overdue';
    if (diffDays <= 7) return 'soon';
    return 'ok';
  }

  togglePanel(panelId: PanelId): void {
    this.layoutService.toggle(panelId);
  }

  dropPanel(event: any): void {
    if (event.previousIndex !== event.currentIndex) {
      moveItemInArray(this.layoutService.orderedPanels, event.previousIndex, event.currentIndex);
      this.layoutService.saveOrder();
    }
  }

  sortEstimations(by: string): void {
    this.budgetEstimationSortBy = by as any;
    this.budgetEstimations = this.applyEstimationSort(this.budgetEstimationService.current);
    this.cdr.markForCheck();
  }

  dropEstimation(event: any): void {
    if (event.previousIndex !== event.currentIndex) {
      const moved = [...this.budgetEstimations];
      moveItemInArray(moved, event.previousIndex, event.currentIndex);
      const orders = moved.map((e, i) => ({ id: e.id, orderIndex: i }));
      this.budgetEstimationService.updateOrder(orders).pipe(takeUntil(this.unsubscribe$)).subscribe();
    }
  }

  sortCapitals(by: string): void {
    this.capitalSortBy = by as any;
    this.capitals = this.applyCapitalSort(this.capitalService.current);
    this.cdr.markForCheck();
  }

  dropCapital(event: any): void {
    if (event.previousIndex !== event.currentIndex) {
      const moved = [...this.capitals];
      moveItemInArray(moved, event.previousIndex, event.currentIndex);
      const orders = moved.map((c, i) => ({ id: c.id, orderIndex: i }));
      this.capitalService.updateOrder({ capitals: orders }).pipe(takeUntil(this.unsubscribe$)).subscribe();
    }
  }

  sortGoals(by: string): void {
    this.goalSortBy = by as any;
    this.goals = this.applyGoalSort(this.goalService.current);
    this.cdr.markForCheck();
  }

  dropGoal(event: any): void {
    if (event.previousIndex !== event.currentIndex) {
      const moved = [...this.goals];
      moveItemInArray(moved, event.previousIndex, event.currentIndex);
      const orders = moved.map((g, i) => ({ id: g.id, orderIndex: i }));
      this.goalService.updateOrder(orders).pipe(takeUntil(this.unsubscribe$)).subscribe();
    }
  }

  sortDebts(by: string): void {
    this.debtSortBy = by as any;
    this.debts = this.applyDebtSort(this.debtService.current);
    this.cdr.markForCheck();
  }

  dropDebt(event: any): void {
    if (event.previousIndex !== event.currentIndex) {
      const moved = [...this.debts];
      moveItemInArray(moved, event.previousIndex, event.currentIndex);
      const orders = moved.map((d, i) => ({ id: d.id, orderIndex: i }));
      this.debtService.updateOrder(orders).pipe(takeUntil(this.unsubscribe$)).subscribe();
    }
  }

  sortExpenses(by: string): void {
    this.expenseSortBy = by as any;
    this.expenses = this.applyExpenseSort(this.expenseService.objects);
    this.cdr.markForCheck();
  }

  dropExpense(event: any): void {
    if (event.previousIndex !== event.currentIndex) {
      const moved = [...this.expenses];
      moveItemInArray(moved, event.previousIndex, event.currentIndex);
      this.expenses = moved;
      this.cdr.markForCheck();
    }
  }

  sortIncomes(by: string): void {
    this.incomeSortBy = by as any;
    this.incomeByCategoryList = this.applyIncomeSort(this.computeIncomesByCategory());
    this.cdr.markForCheck();
  }

  dropIncome(event: any): void {
    if (event.previousIndex !== event.currentIndex) {
      const moved = [...this.incomeByCategoryList];
      moveItemInArray(moved, event.previousIndex, event.currentIndex);
      this.incomeByCategoryList = moved;
      this.cdr.markForCheck();
    }
  }

  openEstimationDialog(estimation?: BudgetEstimation): void {
    const data: BudgetEstimationDialogData = { estimation, capitals: this.capitals };
    const ref = this.dialogService.open(BudgetEstimationDialogComponent, { data });
    ref.afterClosed$.pipe(takeUntil(this.unsubscribe$)).subscribe(result => {
      if (!result) return;
      if (estimation) {
        const req: UpdateBudgetEstimationRequest = result;
        this.budgetEstimationService.update(estimation.id, req).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => this.popup.success('Estimation updated'),
          error: () => this.popup.error('Failed to update estimation')
        });
      } else {
        const req: CreateBudgetEstimationRequest = result;
        this.budgetEstimationService.create(req).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => this.popup.success('Estimation added'),
          error: () => this.popup.error('Failed to add estimation')
        });
      }
    });
  }

  copyEstimation(source: BudgetEstimation): void {
    const data: BudgetEstimationDialogData = { estimation: source, capitals: this.capitals, mode: 'copy' };
    const ref = this.dialogService.open(BudgetEstimationDialogComponent, { data });

    ref.afterClosed$.pipe(takeUntil(this.unsubscribe$)).subscribe(result => {
      if (!result) return;
      const req: CreateBudgetEstimationRequest = result;
      this.budgetEstimationService.create(req).pipe(takeUntil(this.unsubscribe$)).subscribe({
        next: () => this.popup.success('Estimation copied'),
        error: () => this.popup.error('Failed to copy estimation')
      });
    });
  }

  deleteEstimation(estimation: BudgetEstimation): void {
    this.budgetEstimationService.delete(estimation.id).pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: () => this.popup.success('Estimation deleted'),
      error: () => this.popup.error('Failed to delete estimation')
    });
  }

  toggleEstimationCompleted(estimation: BudgetEstimation, event: Event): void {
    event.stopPropagation();
    const checked = (event.target as HTMLInputElement).checked;
    const req: UpdateBudgetEstimationRequest = {
      description: estimation.description,
      budgetAmount: estimation.budgetAmount,
      budgetCurrency: estimation.budgetCurrency,
      capitalId: estimation.capitalId,
      isCompleted: checked
    };
    this.budgetEstimationService.update(estimation.id, req).pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: () => this.popup.success('Estimation updated'),
      error: () => this.popup.error('Failed to update estimation')
    });
  }

  openGoalDialog(goal?: Goal): void {
    const data: GoalDialogData = { goal };
    const ref = this.dialogService.open(GoalDialogComponent, { data });
    ref.afterClosed$.pipe(takeUntil(this.unsubscribe$)).subscribe(result => {
      if (!result) return;
      if (goal) {
        const req: UpdateGoalRequest = { ...result, isCompleted: result.isCompleted ?? goal.isCompleted };
        this.goalService.update(goal.id, req).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => this.popup.success('Goal updated'),
          error: () => this.popup.error('Failed to update goal')
        });
      } else {
        this.goalService.create(result).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => this.popup.success('Goal added'),
          error: () => this.popup.error('Failed to add goal')
        });
      }
    });
  }

  deleteGoal(goal: Goal): void {
    this.goalService.delete(goal.id).pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: () => this.popup.success('Goal deleted'),
      error: () => this.popup.error('Failed to delete goal')
    });
  }

  openDebtDialog(debt?: Debt): void {
    const data: DebtDialogData = { debt, capitals: this.capitals };
    const ref = this.dialogService.open(DebtDialogComponent, { data });
    ref.afterClosed$.pipe(takeUntil(this.unsubscribe$)).subscribe(result => {
      if (!result) return;
      if (debt) {
        const req: UpdateDebtRequest = {
          item: result.item,
          amount: result.amount,
          currency: result.currency,
          source: result.source,
          recipient: result.recipient,
          borrowedAt: result.borrowedAt,
          deadlineAt: result.deadlineAt,
          note: result.note,
          isPaid: result.isPaid ?? debt.isPaid,
          payFromCapitalId: result.payFromCapitalId ?? null
        };
        this.debtService.update(debt.id, req).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => this.popup.success('Debt updated'),
          error: () => this.popup.error('Failed to update debt')
        });
      } else {
        const req: CreateDebtRequest = {
          item: result.item,
          amount: result.amount,
          currency: result.currency,
          source: result.source,
          recipient: result.recipient,
          borrowedAt: result.borrowedAt,
          deadlineAt: result.deadlineAt,
          note: result.note,
          capitalId: result.capitalId ?? null
        };
        this.debtService.create(req).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => this.popup.success('Debt added'),
          error: () => this.popup.error('Failed to add debt')
        });
      }
    });
  }

  deleteDebt(debt: Debt): void {
    this.debtService.delete(debt.id).pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: () => this.popup.success('Debt deleted'),
      error: () => this.popup.error('Failed to delete debt')
    });
  }

  borrowedAmount(capitalId: number): number {
    return this.debts
      .filter(d => d.capitalId === capitalId && !d.isPaid)
      .reduce((sum, d) => sum + d.amount, 0);
  }

  deleteExpense(id: number): void {
    this.expenseService.delete(id).pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: () => { this.popup.success('Expense deleted'); this.capitalService.refresh(); },
      error: () => this.popup.error('Failed to delete expense')
    });
  }

  deleteIncome(id: number): void {
    this.incomeService.delete(id).pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: () => { this.popup.success('Income deleted'); this.capitalService.refresh(); },
      error: () => this.popup.error('Failed to delete income')
    });
  }

  openCapitalDetail(cap: CapitalResponse): void {
    const ref = this.dialogService.open(CapitalDetailsComponent, {
      data: { capital: cap, currencyOptions: getCurrencies({ excludeNone: true }), exchanges: this.exchanges }
    });
    ref.afterClosed$.pipe(takeUntil(this.unsubscribe$)).subscribe((result: UpdateCapitalRequest | null) => {
      if (!result) return;
      this.capitalService.update(result.id, result).pipe(takeUntil(this.unsubscribe$)).subscribe({
        next: () => this.popup.success('Capital updated'),
        error: () => this.popup.error('Failed to update capital')
      });
    });
  }

  deleteTransfer(id: number): void {
    this.transferService.delete(id).pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: () => this.popup.success('Transfer reversed and deleted'),
      error: () => this.popup.error('Failed to delete transfer')
    });
  }

  transfersForCapital(capitalId: number): TransferResponse[] {
    return this.transfers.filter(t => t.sourceCapitalId === capitalId || t.destinationCapitalId === capitalId);
  }

  openTransferDialog(sourceCapitalId?: number): void {
    const data: TransferDialogData = { capitals: this.capitals, exchanges: this.exchanges, sourceCapitalId: sourceCapitalId ?? null };
    const ref = this.dialogService.open(TransferDialogComponent, { data });
    ref.afterClosed$.pipe(takeUntil(this.unsubscribe$)).subscribe(result => {
      if (!result) return;
      this.transferService.create(result).pipe(takeUntil(this.unsubscribe$)).subscribe({
        next: () => this.popup.success('Transfer completed'),
        error: () => this.popup.error('Failed to create transfer')
      });
    });
  }

  openAddCapital(): void {
    const ref = this.dialogService.open(AddCapitalDialogComponent, {
      data: getCurrencies({ excludeNone: true })
    });
    ref.afterClosed$.pipe(takeUntil(this.unsubscribe$)).subscribe((result: AddCapitalRequest | null) => {
      if (!result) return;
      this.capitalService.create(result).pipe(takeUntil(this.unsubscribe$)).subscribe({
        next: () => this.popup.success('Capital added'),
        error: () => this.popup.error('Failed to add capital')
      });
    });
  }

  incomeByCategoryList: { name: string; total: number; count: number; items: IncomeResponse[] }[] = [];

  expandedCapitalId: number | null = null;
  expandedExpenseCats = new Set<number>();
  expandedIncomeCats = new Set<string>();

  toggleExpenseCat(catId: number): void {
    this.expandedExpenseCats.has(catId) ? this.expandedExpenseCats.delete(catId) : this.expandedExpenseCats.add(catId);
  }

  toggleIncomeCat(name: string): void {
    this.expandedIncomeCats.has(name) ? this.expandedIncomeCats.delete(name) : this.expandedIncomeCats.add(name);
  }

  showTransfers(cap: CapitalResponse): void {
    this.expandedCapitalId = this.expandedCapitalId === cap.id ? null : cap.id;
  }

  get totalExpenses(): number {
    return this.expenses.reduce((sum, c) => sum + c.categorySum, 0);
  }

  get totalIncomes(): number {
    return this.incomes.reduce((sum, i) => sum + i.amount, 0);
  }

  get totalIncomesConverted(): number {
    return this.incomes.length > 0 ? this.incomes.reduce(
      (sum, i) => sum + convertCurrency(i.amount, this.capitalCurrencyOf(i.capitalId), this.userCurrency, this.exchanges), 0
    ) : 0;
  }

  get totalIncomesInUah(): number {
    return this.incomes.length > 0 ? this.incomes.reduce(
      (sum, i) => sum + convertCurrency(i.amount, this.capitalCurrencyOf(i.capitalId), CurrencyType.UAH.toString(), this.exchanges), 0
    ) : 0;
  }

  openQuickExpense(editItem?: any): void {
    const ref = this.dialogService.open(QuickTransactionDialogComponent, {
      data: { type: 'expense', capitals: this.capitals, categories: this.expenseCategoriesList, editItem }
    });
    ref.afterClosed$.pipe(takeUntil(this.unsubscribe$)).subscribe(result => {
      if (!result) return;
      if (result._edit) {
        this.expenseService.update({ id: result.id, categoryId: result.categoryId, amount: result.amount, purpose: result.purpose, date: result.date }).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => { this.popup.success('Expense updated'); this.capitalService.refresh(); },
          error: () => this.popup.error('Failed to update expense')
        });
      } else {
        this.expenseService.create(result).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => { this.popup.success('Expense added'); this.capitalService.refresh(); },
          error: () => this.popup.error('Failed to add expense')
        });
      }
    });
  }

  openQuickIncome(editItem?: any): void {
    const ref = this.dialogService.open(QuickTransactionDialogComponent, {
      data: { type: 'income', capitals: this.capitals, categories: this.incomeCategories, editItem }
    });
    ref.afterClosed$.pipe(takeUntil(this.unsubscribe$)).subscribe(result => {
      if (!result) return;
      if (result._edit) {
        this.incomeService.update({ id: result.id, categoryId: result.categoryId, amount: result.amount, purpose: result.purpose, paymentDate: result.date }).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => { this.popup.success('Income updated'); this.capitalService.refresh(); },
          error: () => this.popup.error('Failed to update income')
        });
      } else {
        this.incomeService.create(result).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => { this.popup.success('Income added'); this.capitalService.refresh(); },
          error: () => this.popup.error('Failed to add income')
        });
      }
    });
  }

  loading(key: SectionKey): boolean {
    return this.sectionLoading.isLoading(key);
  }

  ngOnDestroy(): void {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }

  private subscribeToStores(): void {
    this.userSettingsService.settings$.pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => {
        this.userSettings = data;
        this.cdr.markForCheck();
      }
    });

    this.capitalService.capitals$.pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => {
        this.capitals = this.applyCapitalSort(data);
        this.cdr.markForCheck();
      }
    });

    this.budgetEstimationService.estimations$
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe({
        next: data => {
          this.budgetEstimations = this.applyEstimationSort(data);
          this.cdr.markForCheck();
        },
        error: () => this.cdr.markForCheck()
      });

    this.categoryService.categories$.pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => {
        this.expenseCategoriesList = data.filter(c => c.type === CategoryType.Expenses);
        this.incomeCategories = data.filter(c => c.type === CategoryType.Incomes);
        this.cdr.markForCheck();
      }
    });

    this.expenseService.expenses$.pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => {
        this.expenses = this.applyExpenseSort(data);
        this.cdr.markForCheck();
      }
    });

    this.incomeService.incomes$.pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => {
        this.incomes = data;
        this.incomeByCategoryList = this.applyIncomeSort(this.computeIncomesByCategory()); // TODO refactor to backend as incomes
        this.cdr.markForCheck();
      }
    });

    this.goalService.goals$.pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => {
        this.goals = this.applyGoalSort(data);
        this.cdr.markForCheck();
      }
    });

    this.debtService.debts$.pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => {
        this.debts = this.applyDebtSort(data);
        this.cdr.markForCheck();
      }
    });
  }

  private applyCapitalSort(data: CapitalResponse[]): CapitalResponse[] {
    const arr = [...data];
    switch (this.capitalSortBy) {
      case 'balance-desc': return arr.sort((a, b) => b.balance - a.balance);
      case 'balance-asc':  return arr.sort((a, b) => a.balance - b.balance);
      case 'name':         return arr.sort((a, b) => a.name.localeCompare(b.name));
      default:             return arr;
    }
  }

  private applyEstimationSort(data: BudgetEstimation[]): BudgetEstimation[] {
    const arr = [...data];
    switch (this.budgetEstimationSortBy) {
      case 'amount-desc': return arr.sort((a, b) => b.budgetAmount - a.budgetAmount);
      case 'amount-asc':  return arr.sort((a, b) => a.budgetAmount - b.budgetAmount);
      case 'name':        return arr.sort((a, b) => a.description.localeCompare(b.description));
      case 'custom':      return arr.sort((a, b) => a.orderIndex - b.orderIndex);
      default:            return arr;
    }
  }

  private applyGoalSort(data: Goal[]): Goal[] {
    const arr = [...data];
    switch (this.goalSortBy) {
      case 'progress-desc': return arr.sort((a, b) => this.goalProgress(b) - this.goalProgress(a));
      case 'progress-asc':  return arr.sort((a, b) => this.goalProgress(a) - this.goalProgress(b));
      case 'name':          return arr.sort((a, b) => a.title.localeCompare(b.title));
      case 'custom':        return arr.sort((a, b) => a.orderIndex - b.orderIndex);
      default:              return arr;
    }
  }

  private applyDebtSort(data: Debt[]): Debt[] {
    const arr = [...data];
    switch (this.debtSortBy) {
      case 'amount-desc': return arr.sort((a, b) => b.amount - a.amount);
      case 'amount-asc':  return arr.sort((a, b) => a.amount - b.amount);
      case 'name':        return arr.sort((a, b) => a.item.localeCompare(b.item));
      case 'custom':      return arr.sort((a, b) => a.orderIndex - b.orderIndex);
      default:            return arr;
    }
  }

  private applyExpenseSort(data: ExpenseCategoryResponse[]): ExpenseCategoryResponse[] {
    const arr = [...data];
    switch (this.expenseSortBy) {
      case 'amount-desc': return arr.sort((a, b) => b.categorySum - a.categorySum);
      case 'amount-asc':  return arr.sort((a, b) => a.categorySum - b.categorySum);
      case 'name':        return arr.sort((a, b) => a.name.localeCompare(b.name));
      case 'default':     return arr.sort((a, b) => b.percentage - a.percentage);
      default:            return arr;
    }
  }

  private applyIncomeSort<T extends { name: string; total: number }>(data: T[]): T[] {
    const arr = [...data];
    switch (this.incomeSortBy) {
      case 'amount-desc':
      case 'default':     return arr.sort((a, b) => b.total - a.total);
      case 'amount-asc':  return arr.sort((a, b) => a.total - b.total);
      case 'name':        return arr.sort((a, b) => a.name.localeCompare(b.name));
      default:            return arr;
    }
  }

  private computeIncomesByCategory(): { name: string; total: number; count: number; items: IncomeResponse[] }[] {
    const map = new Map<number, { name: string; total: number; count: number; items: IncomeResponse[] }>();
    for (const inc of this.incomes) {
      const cat = this.incomeCategories.find(c => c.id === inc.categoryId);
      const entry = map.get(inc.categoryId) ?? { name: cat?.name ?? 'Unknown', total: 0, count: 0, items: [] };
      entry.total += inc.amount;
      entry.count++;
      entry.items.push(inc);
      map.set(inc.categoryId, entry);
    }
    return [...map.values()].sort((a, b) => b.total - a.total);
  }

  private getBudgetEstimationRequest(): QueryTimeRange {
    switch (this.planFilter) {
      case 'day':
        return {
          periodStart: this.selectedDate,
          periodEnd: this.selectedDate
        };
      case 'month':
        return {
          periodStart: new Date(this.selectedDate.getFullYear(), this.selectedDate.getMonth(), 1),
          periodEnd: new Date(this.selectedDate.getFullYear(), this.selectedDate.getMonth() + 1,0)
        };
      case 'year':
        return {
          periodStart: new Date(this.selectedDate.getFullYear(), 0, 1),
          periodEnd: new Date(this.selectedDate.getFullYear(), 11, 31)
        };
    }
  }
}
