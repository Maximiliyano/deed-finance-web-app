import {ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit} from '@angular/core';
import {Subject, skip, takeUntil} from 'rxjs';
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
import {SalaryDialogComponent, SalaryDialogData} from './components/salary-dialog/salary-dialog.component';
import {Exchange} from '../../core/models/exchange-model';
import {CategoryType} from '../../core/types/category-type';
import {CapitalDetailsComponent} from '../capital/components/capital-details/capital-details.component';
import {getCurrencies} from '../../shared/components/currency/functions/get-currencies.component';
import {UpdateCapitalRequest} from '../capital/models/update-capital-request';
import {SectionLoadingService} from '../../shared/services/section-loading.service';

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

  get salary(): number {
    return this.userSettings?.salary ?? 0;
  }

  get loss(): number {
    return this.estimations.reduce(
      (sum, e) => sum + this.convertToSalaryCurrency(e.budgetAmount, e.budgetCurrency), 0
    );
  }

  get profit(): number {
    return this.salary - this.loss;
  }

  get isProfitable(): boolean {
    return this.profit >= 0;
  }

  get lossPercent(): number {
    return this.salary > 0 ? Math.min(100, Math.round((this.loss / this.salary) * 100)) : 0;
  }

  get profitPercent(): number {
    return Math.max(0, 100 - this.lossPercent);
  }

  budgetPercent(amount: number, fromCurrency?: string): number {
    if (!this.salary) return 0;
    const converted = fromCurrency ? this.convertToSalaryCurrency(amount, fromCurrency) : amount;
    return Math.min(100, Math.round((converted / this.salary) * 100));
  }

  convertToSalaryCurrency(amount: number, fromCurrency: string): number {
    if (fromCurrency === this.currency) return amount;

    const direct = this.exchanges.find(
      e => e.nationalCurrency === this.currency && e.targetCurrency === fromCurrency
    );
    if (direct) return amount * direct.sale;

    const reverse = this.exchanges.find(
      e => e.nationalCurrency === fromCurrency && e.targetCurrency === this.currency
    );
    return reverse && reverse.buy > 0 ? amount / reverse.buy : amount;
  }

  get showDualCurrency(): boolean {
    return this.currency !== 'UAH';
  }

  convertToUah(amount: number, fromCurrency: string): number {
    if (fromCurrency === 'UAH') return amount;

    const direct = this.exchanges.find(
      e => e.nationalCurrency === 'UAH' && e.targetCurrency === fromCurrency
    );
    if (direct) return amount * direct.sale;

    const reverse = this.exchanges.find(
      e => e.nationalCurrency === fromCurrency && e.targetCurrency === 'UAH'
    );
    return reverse && reverse.buy > 0 ? amount / reverse.buy : amount;
  }

  get lossInUah(): number {
    return this.estimations.reduce(
      (sum, e) => sum + this.convertToUah(e.budgetAmount, e.budgetCurrency), 0
    );
  }

  get salaryInUah(): number {
    return this.convertToUah(this.salary, this.currency);
  }

  get profitInUah(): number {
    return this.salaryInUah - this.lossInUah;
  }

  goalProgress(goal: Goal): number {
    if (!goal.targetAmount) return 0;
    return Math.min(100, Math.round((goal.currentAmount / goal.targetAmount) * 100));
  }

  goalSuggestion(goal: Goal): { monthly: number; months: number; feasible: boolean; overdue: boolean } | null {
    if (goal.isCompleted || !goal.deadline || !goal.targetAmount) return null;
    const remaining = this.convertToSalaryCurrency(goal.targetAmount - goal.currentAmount, goal.currency);
    if (remaining <= 0) return null;
    const now = new Date();
    const deadline = new Date(goal.deadline);
    const months = Math.max(0, (deadline.getFullYear() - now.getFullYear()) * 12 + deadline.getMonth() - now.getMonth());
    if (months <= 0) return { monthly: remaining, months: 0, feasible: false, overdue: true };
    const monthly = Math.ceil(remaining / months);
    return { monthly, months, feasible: this.profit >= monthly, overdue: false };
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

  openSalaryDialog(): void {
    const data: SalaryDialogData = { salary: this.salary, currency: this.currency };
    const ref = this.dialogService.open(SalaryDialogComponent, { data });
    ref.afterClosed$.pipe(takeUntil(this.unsubscribe$)).subscribe(result => {
      if (!result) return;
      this.userSettingsService.upsert(result as unknown as UserSettings)
        .pipe(takeUntil(this.unsubscribe$))
        .subscribe({
          next: () => { this.popup.success('Salary updated'); this.loadData(); },
          error: () => this.popup.error('Failed to save salary')
        });
    });
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
    switch (by) {
      case 'amount-desc':
        this.estimations = [...this.estimations].sort((a, b) => b.budgetAmount - a.budgetAmount);
        break;
      case 'amount-asc':
        this.estimations = [...this.estimations].sort((a, b) => a.budgetAmount - b.budgetAmount);
        break;
      case 'name':
        this.estimations = [...this.estimations].sort((a, b) => a.description.localeCompare(b.description));
        break;
      case 'custom':
        this.estimations = [...this.estimations].sort((a, b) => a.orderIndex - b.orderIndex);
        break;
    }
    this.cdr.markForCheck();
  }

  dropEstimation(event: any): void {
    if (event.previousIndex !== event.currentIndex) {
      moveItemInArray(this.estimations, event.previousIndex, event.currentIndex);
      const orders = this.estimations.map((e, i) => ({ id: e.id, orderIndex: i }));
      this.estimationService.updateOrder(orders).pipe(takeUntil(this.unsubscribe$)).subscribe();
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
          next: () => {
            this.popup.success('Estimation updated');
            this.loadData();
          },
          error: () => this.popup.error('Failed to update estimation')
        });
      } else {
        const req: CreateBudgetEstimationRequest = result;
        this.estimationService.create(req).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => {
            this.popup.success('Estimation added');
            this.loadData();
          },
          error: () => this.popup.error('Failed to add estimation')
        });
      }
    });
  }

  deleteEstimation(estimation: BudgetEstimation): void {
    this.estimationService.delete(estimation.id).pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: () => {
        this.estimations = this.estimations.filter(e => e.id !== estimation.id);
        this.popup.success('Estimation deleted');
        this.cdr.markForCheck();
      },
      error: () => this.popup.error('Failed to delete estimation')
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
          next: () => {
            this.popup.success('Goal updated');
            this.loadData();
          },
          error: () => this.popup.error('Failed to update goal')
        });
      } else {
        this.goalService.create(result).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => {
            this.popup.success('Goal added');
            this.loadData();
          },
          error: () => this.popup.error('Failed to add goal')
        });
      }
    });
  }

  deleteGoal(goal: Goal): void {
    this.goalService.delete(goal.id).pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: () => {
        this.goals = this.goals.filter(g => g.id !== goal.id);
        this.popup.success('Goal deleted');
        this.cdr.markForCheck();
      },
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
          next: () => {
            this.popup.success('Debt updated');
            this.loadData();
          },
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
          next: () => {
            this.popup.success('Debt added');
            this.loadData();
          },
          error: () => this.popup.error('Failed to add debt')
        });
      }
    });
  }

  deleteDebt(debt: Debt): void {
    this.debtService.delete(debt.id).pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: () => {
        this.debts = this.debts.filter(d => d.id !== debt.id);
        this.popup.success('Debt deleted');
        this.cdr.markForCheck();
      },
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
      next: () => { this.popup.success('Expense deleted'); this.loadData(); },
      error: () => this.popup.error('Failed to delete expense')
    });
  }

  deleteIncome(id: number): void {
    this.incomeService.delete(id).pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: () => { this.popup.success('Income deleted'); this.loadData(); },
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
        next: () => { this.popup.success('Capital updated'); this.loadData(); },
        error: () => this.popup.error('Failed to update capital')
      });
    });
  }

  deleteTransfer(id: number): void {
    this.transferService.delete(id).pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: () => { this.popup.success('Transfer reversed and deleted'); this.loadData(); },
      error: () => this.popup.error('Failed to delete transfer')
    });
  }

  transfersForCapital(capitalId: number): TransferResponse[] {
    return this.transfers.filter(t => t.sourceCapitalId === capitalId || t.destinationCapitalId === capitalId);
  }

  openTransferDialog(): void {
    const data: TransferDialogData = { capitals: this.capitals, exchanges: this.exchanges };
    const ref = this.dialogService.open(TransferDialogComponent, { data });
    ref.afterClosed$.pipe(takeUntil(this.unsubscribe$)).subscribe(result => {
      if (!result) return;
      this.transferService.create(result).pipe(takeUntil(this.unsubscribe$)).subscribe({
        next: () => { this.popup.success('Transfer completed'); this.loadData(); },
        error: () => this.popup.error('Failed to create transfer')
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


  openQuickExpense(editItem?: any): void {
    const ref = this.dialogService.open(QuickTransactionDialogComponent, {
      data: { type: 'expense', capitals: this.capitals, categories: this.expenseCategoriesList, editItem }
    });
    ref.afterClosed$.pipe(takeUntil(this.unsubscribe$)).subscribe(result => {
      if (!result) return;
      if (result._edit) {
        this.expenseService.update({ id: result.id, categoryId: result.categoryId, amount: result.amount, purpose: result.purpose, date: result.date }).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => { this.popup.success('Expense updated'); this.loadData(); },
          error: () => this.popup.error('Failed to update expense')
        });
      } else {
        this.expenseService.create(result).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => { this.popup.success('Expense added'); this.loadData(); },
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
          next: () => { this.popup.success('Income updated'); this.loadData(); },
          error: () => this.popup.error('Failed to update income')
        });
      } else {
        this.incomeService.create(result).pipe(takeUntil(this.unsubscribe$)).subscribe({
          next: () => { this.popup.success('Income added'); this.loadData(); },
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

    this.sectionLoading.isLoading$('settings').pipe(skip(1), takeUntil(this.unsubscribe$))
      .subscribe(() => this.cdr.markForCheck());
    this.sectionLoading.isLoading$('capitals').pipe(skip(1), takeUntil(this.unsubscribe$))
      .subscribe(() => this.cdr.markForCheck());
    this.sectionLoading.isLoading$('estimations').pipe(skip(1), takeUntil(this.unsubscribe$))
      .subscribe(() => this.cdr.markForCheck());
    this.sectionLoading.isLoading$('goals').pipe(skip(1), takeUntil(this.unsubscribe$))
      .subscribe(() => this.cdr.markForCheck());
    this.sectionLoading.isLoading$('debts').pipe(skip(1), takeUntil(this.unsubscribe$))
      .subscribe(() => this.cdr.markForCheck());
    this.sectionLoading.isLoading$('expenses').pipe(skip(1), takeUntil(this.unsubscribe$))
      .subscribe(() => this.cdr.markForCheck());
    this.sectionLoading.isLoading$('incomes').pipe(skip(1), takeUntil(this.unsubscribe$))
      .subscribe(() => this.cdr.markForCheck());
    this.sectionLoading.isLoading$('transfers').pipe(skip(1), takeUntil(this.unsubscribe$))
      .subscribe(() => this.cdr.markForCheck());
    this.sectionLoading.isLoading$('exchanges').pipe(skip(1), takeUntil(this.unsubscribe$))
      .subscribe(() => this.cdr.markForCheck());

    this.authService.me()
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe({ next: user => { this.user = user; } });

    this.loadData();

    this.route.fragment.pipe(takeUntil(this.unsubscribe$)).subscribe(fragment => {
      if (fragment) {
        setTimeout(() => {
          const el = document.getElementById(fragment);
          if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 300);
      }
    });
  }

  private loadData(): void {
    this.userSettingsService.get().pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => { this.userSettings = data; this.cdr.markForCheck(); },
      error: () => this.cdr.markForCheck()
    });

    this.capitalService.getAll({ searchTerm: null, sortBy: null, sortDirection: null, filterBy: null })
      .pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => { this.capitals = data; this.cdr.markForCheck(); },
      error: () => this.cdr.markForCheck()
    });

    this.exchangeService.getLatest().pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => { this.exchanges = data; this.cdr.markForCheck(); },
      error: () => this.cdr.markForCheck()
    });

    this.estimationService.getAll().pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => { this.estimations = data; this.cdr.markForCheck(); },
      error: () => this.cdr.markForCheck()
    });

    this.goalService.getAll().pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => { this.goals = data; this.cdr.markForCheck(); },
      error: () => this.cdr.markForCheck()
    });

    this.debtService.getAll().pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => { this.debts = data; this.cdr.markForCheck(); },
      error: () => this.cdr.markForCheck()
    });

    this.expenseService.getAllByCategories().pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => { this.expenseCategories = data; this.cdr.markForCheck(); },
      error: () => this.cdr.markForCheck()
    });

    this.incomeService.getAll().pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => {
        this.incomes = data.incomes;
        this.incomeCategories = data.categories;
        this.incomeByCategoryList = this.computeIncomesByCategory();
        this.cdr.markForCheck();
      },
      error: () => this.cdr.markForCheck()
    });

    this.categoryService.getAll(CategoryType.Expenses).pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => { this.expenseCategoriesList = data; this.cdr.markForCheck(); },
      error: () => this.cdr.markForCheck()
    });

    this.transferService.getAll().pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: data => { this.transfers = data; this.cdr.markForCheck(); },
      error: () => this.cdr.markForCheck()
    });
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
