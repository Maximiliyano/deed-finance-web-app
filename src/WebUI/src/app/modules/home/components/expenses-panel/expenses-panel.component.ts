import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { moveItemInArray } from '@angular/cdk/drag-drop';
import { catchError, of, switchMap } from 'rxjs';
import { CategoryType } from '../../../../core/types/category-type';
import { DialogService } from '../../../../shared/components/dialogs/services/dialog.service';
import { PanelStateComponent } from '../../../../shared/components/panel-state/panel-state.component';
import {
  AMOUNT_SORT_OPTIONS,
  AmountSort,
  SortToolbarComponent
} from '../../../../shared/components/sort-toolbar/sort-toolbar.component';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SectionLoadingService } from '../../../../shared/services/section-loading.service';
import { SharedModule } from '../../../../shared/shared.module';
import { CapitalService } from '../../../capital/services/capital.service';
import { CategoryResponse } from '../../../category/models/category-model';
import { CategoryService } from '../../../category/services/category.service';
import { ExpenseCategoryResponse } from '../../../expense/models/expense-category-response';
import { ExpenseService } from '../../../expense/services/expense.service';
import { PeriodState } from '../../models/period-state';
import { DashboardStore } from '../../services/dashboard-store.service';
import { PeriodBarComponent } from '../period-bar/period-bar.component';
import { CategoryBreakdownComponent } from '../category-breakdown/category-breakdown.component';
import { BreakdownGroup, BreakdownItem, ReorderEvent } from '../category-breakdown/category-breakdown.model';
import { QuickAddService } from '../../services/quick-add.service';
import { QuickTransactionDialogComponent } from '../quick-transaction-dialog/quick-transaction-dialog.component';

@Component({
  selector: 'app-expenses-panel',
  templateUrl: './expenses-panel.component.html',
  standalone: true,
  imports: [SharedModule, CategoryBreakdownComponent, SortToolbarComponent, PanelStateComponent, PeriodBarComponent],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExpensesPanelComponent implements OnInit {
  private readonly expenseService = inject(ExpenseService);
  private readonly categoryService = inject(CategoryService);
  private readonly capitalService = inject(CapitalService);
  private readonly dialogService = inject(DialogService);
  private readonly popup = inject(PopupMessageService);
  private readonly sectionLoading = inject(SectionLoadingService);
  protected readonly store = inject(DashboardStore);
  private readonly quickAdd = inject(QuickAddService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly sortOptions = AMOUNT_SORT_OPTIONS;
  protected readonly userCurrency = this.store.userCurrency;
  protected readonly isAnonymous = this.store.isAnonymous;

  protected readonly period = new PeriodState();
  protected readonly sortBy = signal<AmountSort>('default');
  private readonly categories = signal<CategoryResponse[]>([]);
  private readonly expenses = signal<ExpenseCategoryResponse[]>([]);
  private readonly loaded = signal(false);
  private readonly requestPending = toSignal(this.sectionLoading.isLoading$('expenses'), { initialValue: false });

  protected readonly loading = computed(() => !this.loaded() || this.requestPending());
  private readonly visibleExpenses = computed<ExpenseCategoryResponse[]>(() => {
    const ids = this.store.selectedCapitalIds();
    if (ids.size === 0) return this.expenses();

    const filtered = this.expenses()
      .map(category => {
        const items = category.expenses.filter(expense => ids.has(expense.capitalId));

        return { ...category, expenses: items, categorySum: items.reduce((sum, expense) => sum + expense.amount, 0) };
      })
      .filter(category => category.expenses.length > 0);

    const total = filtered.reduce((sum, category) => sum + category.categorySum, 0);

    return filtered.map(category => ({
      ...category,
      percentage: total > 0 ? (category.categorySum / total) * 100 : 0
    }));
  });

  protected readonly total = computed(() =>
    this.visibleExpenses().reduce((sum, category) => sum + category.categorySum, 0)
  );
  protected readonly reorderable = computed(() => this.sortBy() === 'custom' && !this.isAnonymous());
  protected readonly groups = computed<BreakdownGroup[]>(() =>
    this.visibleExpenses().map(category => ({
      key: category.categoryId,
      name: category.name,
      count: category.expenses.length,
      total: category.categorySum,
      percentage: category.percentage,
      items: category.expenses.map(expense => ({ ...expense, categoryId: category.categoryId }))
    }))
  );

  constructor() {
    toObservable(this.period.range)
      .pipe(
        switchMap(range => {
          this.expenseService.setRange(range);
          return this.expenseService.load().pipe(catchError(() => of([])));
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.loaded.set(true));
  }

  ngOnInit(): void {
    this.expenseService.expenses$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(data => this.expenses.set(this.applySort(data)));

    this.categoryService.categories$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(data => this.categories.set(data.filter(category => category.type === CategoryType.Expenses)));

    this.categoryService
      .load()
      .pipe(
        catchError(() => of([])),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe();
  }

  protected sort(by: string): void {
    this.sortBy.set(by as AmountSort);
    this.expenses.set(this.applySort(this.expenseService.current));
  }

  protected reorder(event: ReorderEvent): void {
    if (event.previousIndex === event.currentIndex) return;

    const moved = [...this.expenses()];
    moveItemInArray(moved, event.previousIndex, event.currentIndex);
    this.expenses.set(moved);
  }

  protected addExpense(): void {
    void this.quickAdd.addTransaction('expense');
  }

  protected async manageCategories(): Promise<void> {
    if (await this.quickAdd.manageCategories('expense')) {
      this.expenseService
        .load()
        .pipe(
          catchError(() => of([])),
          takeUntilDestroyed(this.destroyRef)
        )
        .subscribe();
    }
  }

  protected editExpense(item: BreakdownItem): void {
    const editItem = {
      id: item.id,
      capitalId: item.capitalId,
      categoryId: item.categoryId,
      amount: item.amount,
      paymentDate: item.paymentDate,
      purpose: item.purpose,
      tagNames: item.tagNames ?? null
    };

    const ref = this.dialogService.open(QuickTransactionDialogComponent, {
      data: { type: 'expense', capitals: this.store.capitals(), categories: this.categories(), editItem }
    });

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(result => {
      if (!result) return;

      this.expenseService
        .update({
          id: result.id,
          categoryId: result.categoryId,
          amount: result.amount,
          purpose: result.purpose,
          date: result.date,
          tagNames: result.tagNames
        })
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.popup.success('Expense updated');
            this.capitalService.refresh();
          },
          error: () => this.popup.error('Failed to update expense')
        });
    });
  }

  protected deleteExpense(id: number): void {
    this.expenseService
      .delete(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.popup.success('Expense deleted');
          this.capitalService.refresh();
        },
        error: () => this.popup.error('Failed to delete expense')
      });
  }

  private applySort(data: ExpenseCategoryResponse[]): ExpenseCategoryResponse[] {
    const sorted = [...data];

    switch (this.sortBy()) {
      case 'amount-desc':
        return sorted.sort((a, b) => b.categorySum - a.categorySum);
      case 'amount-asc':
        return sorted.sort((a, b) => a.categorySum - b.categorySum);
      case 'name':
        return sorted.sort((a, b) => a.name.localeCompare(b.name));
      case 'default':
        return sorted.sort((a, b) => b.percentage - a.percentage);
      default:
        return sorted;
    }
  }
}
