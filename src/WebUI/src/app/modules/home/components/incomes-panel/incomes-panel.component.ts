import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  effect,
  inject,
  signal
} from '@angular/core';
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
import { IncomeResponse } from '../../../incomes/models/income-response';
import { IncomeService } from '../../../incomes/services/income.service';
import { PeriodState } from '../../models/period-state';
import { DashboardStore } from '../../services/dashboard-store.service';
import { PeriodBarComponent } from '../period-bar/period-bar.component';
import { CategoryBreakdownComponent } from '../category-breakdown/category-breakdown.component';
import { BreakdownGroup, BreakdownItem, ReorderEvent } from '../category-breakdown/category-breakdown.model';
import { BreakdownSearch, searchGroups, sortGroups } from '../category-breakdown/breakdown-search';
import { QuickAddService } from '../../services/quick-add.service';
import { QuickTransactionDialogComponent } from '../quick-transaction-dialog/quick-transaction-dialog.component';

@Component({
  selector: 'app-incomes-panel',
  templateUrl: './incomes-panel.component.html',
  standalone: true,
  imports: [SharedModule, CategoryBreakdownComponent, SortToolbarComponent, PanelStateComponent, PeriodBarComponent],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class IncomesPanelComponent implements OnInit {
  private readonly incomeService = inject(IncomeService);
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
  protected readonly search = new BreakdownSearch();
  private readonly categories = signal<CategoryResponse[]>([]);
  private readonly incomes = signal<IncomeResponse[]>([]);
  private readonly orderedGroups = signal<BreakdownGroup[]>([]);
  private readonly loaded = signal(false);
  private readonly requestPending = toSignal(this.sectionLoading.isLoading$('incomes'), { initialValue: false });

  protected readonly allGroups = this.orderedGroups.asReadonly();
  protected readonly groups = computed<BreakdownGroup[]>(() => {
    const found = searchGroups(this.allGroups(), this.search.term());

    return this.search.active() ? sortGroups(found, this.sortBy()) : found;
  });
  protected readonly loading = computed(() => !this.loaded() || this.requestPending());
  private readonly visibleIncomes = computed(() => {
    const ids = this.store.selectedCapitalIds();

    return ids.size === 0 ? this.incomes() : this.incomes().filter(income => ids.has(income.capitalId));
  });

  private readonly visibleTotal = computed(() => this.visibleIncomes().reduce((sum, income) => sum + income.amount, 0));

  protected readonly total = computed(() => this.groups().reduce((sum, group) => sum + group.total, 0));
  protected readonly allCount = computed(() => this.allGroups().reduce((sum, group) => sum + group.count, 0));
  protected readonly matchCount = computed(() => this.groups().reduce((sum, group) => sum + group.count, 0));
  protected readonly reorderable = computed(
    () => this.sortBy() === 'custom' && !this.isAnonymous() && !this.search.active()
  );

  constructor() {
    effect(() => {
      this.store.selectedCapitalIds();
      this.rebuild();
    });

    toObservable(this.period.range)
      .pipe(
        switchMap(range => {
          this.incomeService.setRange(range);
          return this.incomeService.load().pipe(catchError(() => of([])));
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.loaded.set(true));
  }

  ngOnInit(): void {
    this.incomeService.incomes$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(data => {
      this.incomes.set(data);
      this.rebuild();
    });

    this.categoryService.categories$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(data => {
      this.categories.set(data.filter(category => category.type === CategoryType.Incomes));
      this.rebuild();
    });

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
    this.rebuild();
  }

  protected reorder(event: ReorderEvent): void {
    if (event.previousIndex === event.currentIndex) return;

    const moved = [...this.orderedGroups()];
    moveItemInArray(moved, event.previousIndex, event.currentIndex);
    this.orderedGroups.set(moved);
  }

  protected addIncome(): void {
    void this.quickAdd.addTransaction('income');
  }

  protected async manageCategories(): Promise<void> {
    if (await this.quickAdd.manageCategories('income')) {
      this.incomeService
        .load()
        .pipe(
          catchError(() => of([])),
          takeUntilDestroyed(this.destroyRef)
        )
        .subscribe();
    }
  }

  protected editIncome(item: BreakdownItem): void {
    const editItem = {
      id: item.id,
      capitalId: item.capitalId,
      categoryId: item.categoryId,
      amount: item.amount,
      paymentDate: item.paymentDate,
      purpose: item.purpose
    };

    const ref = this.dialogService.open(QuickTransactionDialogComponent, {
      data: { type: 'income', capitals: this.store.capitals(), categories: this.categories(), editItem }
    });

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(result => {
      if (!result) return;

      this.incomeService
        .update({
          id: result.id,
          categoryId: result.categoryId,
          amount: result.amount,
          purpose: result.purpose,
          paymentDate: result.date
        })
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.popup.success('Income updated');
            this.capitalService.refresh();
          },
          error: () => this.popup.error('Failed to update income')
        });
    });
  }

  protected deleteIncome(id: number): void {
    this.incomeService
      .delete(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.popup.success('Income deleted');
          this.capitalService.refresh();
        },
        error: () => this.popup.error('Failed to delete income')
      });
  }

  private rebuild(): void {
    this.orderedGroups.set(this.applySort(this.groupByCategory()));
  }

  private groupByCategory(): BreakdownGroup[] {
    const total = this.visibleTotal();
    const byCategory = new Map<number, BreakdownGroup>();

    for (const income of this.visibleIncomes()) {
      const group = byCategory.get(income.categoryId) ?? {
        key: income.categoryId,
        name: this.categories().find(category => category.id === income.categoryId)?.name ?? 'Unknown',
        count: 0,
        total: 0,
        percentage: null,
        items: []
      };

      group.total += income.amount;
      group.count++;
      group.items.push({ ...income, tagNames: income.tags ?? null });
      byCategory.set(income.categoryId, group);
    }

    return [...byCategory.values()].map(group => ({
      ...group,
      percentage: total > 0 ? (group.total / total) * 100 : null
    }));
  }

  private applySort(data: BreakdownGroup[]): BreakdownGroup[] {
    const sorted = [...data];

    switch (this.sortBy()) {
      case 'amount-asc':
        return sorted.sort((a, b) => a.total - b.total);
      case 'name':
        return sorted.sort((a, b) => a.name.localeCompare(b.name));
      case 'custom':
        return sorted;
      default:
        return sorted.sort((a, b) => b.total - a.total);
    }
  }
}
