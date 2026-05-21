import {ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit} from '@angular/core';
import {Subject, combineLatest, skip, takeUntil} from 'rxjs';
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
import {ExchangeService} from '../../shared/services/exchange.service';
import {TransferResponse, TransferService} from '../../shared/services/transfer.service';
import {TransferDialogComponent, TransferDialogData} from './components/transfer-dialog/transfer-dialog.component';
import {Exchange} from '../../core/models/exchange-model';
import {CategoryType} from '../../core/types/category-type';
import {CurrencyType} from '../../core/types/currency-type';
import {CapitalDetailsComponent} from '../capital/components/capital-details/capital-details.component';
import {AddCapitalDialogComponent} from '../capital/components/capital-dialog/add-capital-dialog.component';
import {AddCapitalRequest} from '../capital/models/add-capital-request';
import {getCurrencies} from '../../shared/components/currency/functions/get-currencies.component';
import {UpdateCapitalRequest} from '../capital/models/update-capital-request';
import {SectionLoadingService} from '../../shared/services/section-loading.service';
import {convertCurrency} from '../../shared/utils/currency-conversion.util';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrl: './home.component.scss',
    standalone: false,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent implements OnInit, OnDestroy {
  user: User | null = null;
  estimations: BudgetEstimation[] = [];
  goals: Goal[] = [];
  debts: Debt[] = [];
  capitals: CapitalResponse[] = [];
  transfers: TransferResponse[] = [];
  expenseCategories: ExpenseCategoryResponse[] = [];
  incomes: IncomeResponse[] = [];
  incomeCategories: CategoryResponse[] = [];
  expenseCategoriesList: CategoryResponse[] = [];
  userSettings: UserSettings | null = null;
  exchanges: Exchange[] = [];
  isEditMode = false;
  estimationSortBy: 'custom' | 'amount-asc' | 'amount-desc' | 'name' = 'custom';
  capitalSortBy: 'custom' | 'balance-desc' | 'balance-asc' | 'name' = 'custom';
  goalSortBy: 'custom' | 'progress-desc' | 'progress-asc' | 'name' = 'custom';
  debtSortBy: 'custom' | 'amount-desc' | 'amount-asc' | 'name' = 'custom';
  expenseSortBy: 'custom' | 'default' | 'amount-desc' | 'amount-asc' | 'name' = 'default';
  incomeSortBy: 'custom' | 'default' | 'amount-desc' | 'amount-asc' | 'name' = 'default';
  planFilter: 'day' | 'month' | 'year' = 'month';
  selectedDate: Date = new Date();
  showCharts = false;
  chartTab: 'general' | 'expenses' | 'incomes' = 'general';
  selectedCapitalIds = new Set<number>();

  readonly barColors = ['#60a5fa', '#f472b6', '#34d399', '#fb923c', '#38bdf8', '#facc15', '#2dd4bf'];

  private unsubscribe$ = new Subject<void>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly authService: AuthService,
    private readonly estimationService: BudgetEstimationService,
    private readonly goalService: GoalService,
    private readonly debtService: DebtService,
    private readonly userSettingsService: UserSettingsService,
    private readonly capitalService: CapitalService,
    private readonly expenseService: ExpenseService,
    private readonly incomeService: IncomeService,
    private readonly categoryService: CategoryService,
    private readonly dialogService: DialogService,
    private readonly popup: PopupMessageService,
    private readonly exchangeService: ExchangeService,
    private readonly transferService: TransferService,
    readonly layoutService: DashboardLayoutService,
    readonly sectionLoading: SectionLoadingService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  get isAnonymous(): boolean {
    return !this.user;
  }

  get currency(): string {
    return this.userSettings?.currency ?? 'UAH';
  }

  get loss(): number {
    return this.estimations.reduce(
      (sum, e) => sum + this.convertToSalaryCurrency(e.budgetAmount, e.budgetCurrency), 0
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

  budgetPercent(amount: number, fromCurrency?: string): number {
    if (this.totalCapitalAmount <= 0) return 0;
    const converted = fromCurrency ? this.convertToSalaryCurrency(amount, fromCurrency) : amount;
    return Math.min(100, Math.round((converted / this.totalCapitalAmount) * 100));
  }

  convertToSalaryCurrency(amount: number, fromCurrency: string): number {
    return this.convertCurrency(amount, fromCurrency, this.currency);
  }

  convertCurrency(amount: number, fromCurrency: string, toCurrency: string): number {
    return convertCurrency(amount, fromCurrency, toCurrency, this.exchanges);
  }

  get showDualCurrency(): boolean {
    return this.currency !== 'UAH';
  }

  convertToUah(amount: number, fromCurrency: string): number {
    return convertCurrency(amount, fromCurrency, 'UAH', this.exchanges);
  }

  get lossInUah(): number {
    return this.estimations.reduce(
      (sum, e) => sum + this.convertToUah(e.budgetAmount, e.budgetCurrency), 0
    );
  }

  get profitInUah(): number {
    return this.totalIncomesInUah - this.lossInUah;
  }

  get totalCapitalAmount(): number {
    return this.capitals
      .filter(c => c.includeInTotal)
      .reduce((sum, c) => sum + this.convertToSalaryCurrency(c.balance, c.currency), 0);
  }

  get totalCapitalAmountInUah(): number {
    return this.capitals
      .filter(c => c.includeInTotal)
      .reduce((sum, c) => sum + this.convertToUah(c.balance, c.currency), 0);
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
    return this.currency;
  }

  private capitalCurrencyOf(capitalId: number | null | undefined): string {
    if (capitalId == null) return this.chartCurrency;
    const cap = this.capitals.find(c => c.id === capitalId);
    return cap?.currency ?? this.chartCurrency;
  }

  get filteredTotalIncomes(): number {
    const target = this.chartCurrency;
    return this.filteredIncomes.reduce((s, i) =>
      s + this.convertCurrency(i.amount, this.capitalCurrencyOf(i.capitalId), target), 0);
  }

  get filteredLoss(): number {
    const target = this.chartCurrency;
    return this.estimations
      .filter(e => this.capitalAllowed(e.capitalId))
      .reduce((sum, e) => sum + this.convertCurrency(e.budgetAmount, e.budgetCurrency, target), 0);
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
      const converted = this.convertCurrency(inc.amount, this.capitalCurrencyOf(inc.capitalId), target);
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
    const filtered = this.expenseCategories.map(c => {
      const expenses = this.hasCapitalFilter
        ? c.expenses.filter((e: any) => this.capitalAllowed(e.capitalId))
        : c.expenses;
      const sum = expenses.reduce((s: number, e: any) =>
        s + this.convertCurrency(e.amount, this.capitalCurrencyOf(e.capitalId), target), 0);
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
    for (const cat of this.expenseCategories) {
      const expenses = this.hasCapitalFilter
        ? cat.expenses.filter((e: any) => this.capitalAllowed(e.capitalId))
        : cat.expenses;
      for (const e of expenses) {
        sum += this.convertCurrency(e.amount, this.capitalCurrencyOf((e as any).capitalId), target);
      }
    }
    return sum;
  }

  get planFilterLabel(): string {
    return this.planFilter === 'day' ? 'Daily' : this.planFilter === 'year' ? 'Yearly' : 'Monthly';
  }

  setPlanFilter(scope: 'day' | 'month' | 'year'): void {
    this.planFilter = scope;
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
    if (!value) return;
    const d = new Date(`${value}T00:00:00`);
    if (!isNaN(d.getTime())) {
      this.selectedDate = d;
      this.cdr.markForCheck();
    }
  }

  onMonthChange(value: string): void {
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
    const y = typeof value === 'string' ? Number(value) : value;
    if (!Number.isFinite(y) || y < 1900 || y > 2100) return;
    const d = new Date(this.selectedDate);
    d.setFullYear(y);
    this.selectedDate = d;
    this.cdr.markForCheck();
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
    return this.convertToSalaryCurrency(goal.targetAmount - goal.currentAmount, goal.currency);
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

  toggleEditMode(): void {
    this.isEditMode = !this.isEditMode;
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
    this.estimationSortBy = by as any;
    this.estimations = this.applyEstimationSort(this.estimationService.current);
    this.cdr.markForCheck();
  }

  dropEstimation(event: any): void {
    if (event.previousIndex !== event.currentIndex) {
      const moved = [...this.estimations];
      moveItemInArray(moved, event.previousIndex, event.currentIndex);
      const orders = moved.map((e, i) => ({ id: e.id, orderIndex: i }));
      this.estimationService.updateOrder(orders).pipe(takeUntil(this.unsubscribe$)).subscribe();
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
    this.expenseCategories = this.applyExpenseSort(this.expenseService.current);
    this.cdr.markForCheck();
  }

  dropExpense(event: any): void {
    if (event.previousIndex !== event.currentIndex) {
      const moved = [...this.expenseCategories];
      moveItemInArray(moved, event.previousIndex, event.currentIndex);
      this.expenseCategories = moved;
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
        this.estimationService.update(estimation.id, req).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => this.popup.success('Estimation updated'),
          error: () => this.popup.error('Failed to update estimation')
        });
      } else {
        const req: CreateBudgetEstimationRequest = result;
        this.estimationService.create(req).pipe(takeUntil(this.unsubscribe$)).subscribe({
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
      this.estimationService.create(req).pipe(takeUntil(this.unsubscribe$)).subscribe({
        next: () => this.popup.success('Estimation copied'),
        error: () => this.popup.error('Failed to copy estimation')
      });
    });
  }

  deleteEstimation(estimation: BudgetEstimation): void {
    this.estimationService.delete(estimation.id).pipe(takeUntil(this.unsubscribe$)).subscribe({
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
      budgetCurrency: CurrencyType[estimation.budgetCurrency as keyof typeof CurrencyType] as unknown as number,
      capitalId: estimation.capitalId,
      isCompleted: checked
    };
    this.estimationService.update(estimation.id, req).pipe(takeUntil(this.unsubscribe$)).subscribe({
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
    return this.expenseCategories.reduce((sum, c) => sum + c.categorySum, 0);
  }

  get totalIncomes(): number {
    return this.incomes.reduce((sum, i) => sum + i.amount, 0);
  }

  get totalIncomesConverted(): number {
    return this.incomes.reduce(
      (sum, i) => sum + this.convertToSalaryCurrency(i.amount, this.capitalCurrencyOf(i.capitalId)), 0
    );
  }

  get totalIncomesInUah(): number {
    return this.incomes.reduce(
      (sum, i) => sum + this.convertToUah(i.amount, this.capitalCurrencyOf(i.capitalId)), 0
    );
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

  loading(key: string): boolean {
    return this.sectionLoading.isLoading(key as any);
  }

  ngOnInit(): void {
    document.title = 'Deed - Home page';

    combineLatest([
      this.sectionLoading.isLoading$('settings'),
      this.sectionLoading.isLoading$('capitals'),
      this.sectionLoading.isLoading$('estimations'),
      this.sectionLoading.isLoading$('goals'),
      this.sectionLoading.isLoading$('debts'),
      this.sectionLoading.isLoading$('expenses'),
      this.sectionLoading.isLoading$('incomes'),
      this.sectionLoading.isLoading$('transfers'),
      this.sectionLoading.isLoading$('exchanges')
    ]).pipe(skip(1), takeUntil(this.unsubscribe$))
      .subscribe(() => this.cdr.markForCheck());

    this.authService.me()
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe({ next: user => { this.user = user; } });

    this.subscribeToStores();
    this.loadInitialData();

    this.route.fragment.pipe(takeUntil(this.unsubscribe$)).subscribe(fragment => {
      if (fragment) {
        setTimeout(() => {
          const el = document.getElementById(fragment);
          if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 300);
      }
    });
  }

  private subscribeToStores(): void {
    this.userSettingsService.settings$.pipe(takeUntil(this.unsubscribe$)).subscribe(data => {
      this.userSettings = data;
      this.cdr.markForCheck();
    });

    this.capitalService.capitals$.pipe(takeUntil(this.unsubscribe$)).subscribe(data => {
      this.capitals = this.applyCapitalSort(data);
      this.cdr.markForCheck();
    });

    this.estimationService.estimations$.pipe(takeUntil(this.unsubscribe$)).subscribe(data => {
      this.estimations = this.applyEstimationSort(data);
      this.cdr.markForCheck();
    });

    this.goalService.goals$.pipe(takeUntil(this.unsubscribe$)).subscribe(data => {
      this.goals = this.applyGoalSort(data);
      this.cdr.markForCheck();
    });

    this.debtService.debts$.pipe(takeUntil(this.unsubscribe$)).subscribe(data => {
      this.debts = this.applyDebtSort(data);
      this.cdr.markForCheck();
    });

    this.expenseService.categories$.pipe(takeUntil(this.unsubscribe$)).subscribe(data => {
      this.expenseCategories = this.applyExpenseSort(data);
      this.cdr.markForCheck();
    });

    this.incomeService.incomes$.pipe(takeUntil(this.unsubscribe$)).subscribe(data => {
      this.incomes = data;
      this.incomeByCategoryList = this.applyIncomeSort(this.computeIncomesByCategory());
      this.cdr.markForCheck();
    });

    this.incomeService.categories$.pipe(takeUntil(this.unsubscribe$)).subscribe(data => {
      this.incomeCategories = data;
      this.incomeByCategoryList = this.applyIncomeSort(this.computeIncomesByCategory());
      this.cdr.markForCheck();
    });

    this.transferService.transfers$.pipe(takeUntil(this.unsubscribe$)).subscribe(data => {
      this.transfers = data;
      this.cdr.markForCheck();
    });
  }

  private loadInitialData(): void {
    this.userSettingsService.load().pipe(takeUntil(this.unsubscribe$)).subscribe({ error: () => this.cdr.markForCheck() });
    this.capitalService.load({ searchTerm: null, sortBy: null, sortDirection: null, filterBy: null })
      .pipe(takeUntil(this.unsubscribe$)).subscribe({ error: () => this.cdr.markForCheck() });
    this.estimationService.load().pipe(takeUntil(this.unsubscribe$)).subscribe({ error: () => this.cdr.markForCheck() });
    this.goalService.load().pipe(takeUntil(this.unsubscribe$)).subscribe({ error: () => this.cdr.markForCheck() });
    this.debtService.load().pipe(takeUntil(this.unsubscribe$)).subscribe({ error: () => this.cdr.markForCheck() });
    this.expenseService.load().pipe(takeUntil(this.unsubscribe$)).subscribe({ error: () => this.cdr.markForCheck() });
    this.incomeService.load().pipe(takeUntil(this.unsubscribe$)).subscribe({ error: () => this.cdr.markForCheck() });
    this.transferService.load().pipe(takeUntil(this.unsubscribe$)).subscribe({ error: () => this.cdr.markForCheck() });

    this.exchangeService.getLatest().pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => { this.exchanges = data; this.cdr.markForCheck(); },
      error: () => this.cdr.markForCheck()
    });

    this.categoryService.getAll(CategoryType.Expenses).pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => { this.expenseCategoriesList = data; this.cdr.markForCheck(); },
      error: () => this.cdr.markForCheck()
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
    switch (this.estimationSortBy) {
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

  ngOnDestroy(): void {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }
}
