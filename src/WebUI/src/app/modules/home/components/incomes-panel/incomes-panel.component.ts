import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { moveItemInArray } from '@angular/cdk/drag-drop';
import { RouterLink } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { CategoryType } from '../../../../core/types/category-type';
import { DialogService } from '../../../../shared/components/dialogs/services/dialog.service';
import { PanelStateComponent } from '../../../../shared/components/panel-state/panel-state.component';
import { AMOUNT_SORT_OPTIONS, AmountSort, SortToolbarComponent } from '../../../../shared/components/sort-toolbar/sort-toolbar.component';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SectionLoadingService } from '../../../../shared/services/section-loading.service';
import { SharedModule } from '../../../../shared/shared.module';
import { CapitalService } from '../../../capital/services/capital.service';
import { CategoryResponse } from '../../../category/models/category-model';
import { CategoryService } from '../../../category/services/category.service';
import { IncomeResponse } from '../../../incomes/models/income-response';
import { IncomeService } from '../../../incomes/services/income.service';
import { DashboardStore } from '../../services/dashboard-store.service';
import { CategoryBreakdownComponent } from '../category-breakdown/category-breakdown.component';
import { BreakdownGroup, BreakdownItem, ReorderEvent } from '../category-breakdown/category-breakdown.model';
import { QuickTransactionDialogComponent } from '../quick-transaction-dialog/quick-transaction-dialog.component';

@Component({
  selector: 'app-incomes-panel',
  templateUrl: './incomes-panel.component.html',
  styleUrl: './incomes-panel.component.scss',
  standalone: true,
  imports: [SharedModule, RouterLink, CategoryBreakdownComponent, SortToolbarComponent, PanelStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class IncomesPanelComponent implements OnInit {
  private readonly incomeService = inject(IncomeService);
  private readonly categoryService = inject(CategoryService);
  private readonly capitalService = inject(CapitalService);
  private readonly dialogService = inject(DialogService);
  private readonly popup = inject(PopupMessageService);
  private readonly sectionLoading = inject(SectionLoadingService);
  private readonly store = inject(DashboardStore);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly sortOptions = AMOUNT_SORT_OPTIONS;
  protected readonly userCurrency = this.store.userCurrency;
  protected readonly isAnonymous = this.store.isAnonymous;

  protected readonly sortBy = signal<AmountSort>('default');
  private readonly categories = signal<CategoryResponse[]>([]);
  private readonly incomes = signal<IncomeResponse[]>([]);
  private readonly orderedGroups = signal<BreakdownGroup[]>([]);
  private readonly loaded = signal(false);
  private readonly requestPending = toSignal(this.sectionLoading.isLoading$('incomes'), { initialValue: false });

  protected readonly groups = this.orderedGroups.asReadonly();
  protected readonly loading = computed(() => !this.loaded() || this.requestPending());
  protected readonly total = computed(() => this.incomes().reduce((sum, income) => sum + income.amount, 0));
  protected readonly reorderable = computed(() => this.sortBy() === 'custom' && !this.isAnonymous());

  ngOnInit(): void {
    this.incomeService.incomes$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(data => {
        this.incomes.set(data);
        this.rebuild();
      });

    this.categoryService.categories$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(data => {
        this.categories.set(data.filter(category => category.type === CategoryType.Incomes));
        this.rebuild();
      });

    forkJoin([
      this.categoryService.load().pipe(catchError(() => of([]))),
      this.incomeService.load().pipe(catchError(() => of([])))
    ])
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loaded.set(true));
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

  protected openQuickIncome(item?: BreakdownItem): void {
    const editItem = item && {
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

      if (result._edit) {
        this.incomeService
          .update({ id: result.id, categoryId: result.categoryId, amount: result.amount, purpose: result.purpose, paymentDate: result.date })
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => { this.popup.success('Income updated'); this.capitalService.refresh(); },
            error: () => this.popup.error('Failed to update income')
          });
      } else {
        this.incomeService
          .create(result)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => { this.popup.success('Income added'); this.capitalService.refresh(); },
            error: () => this.popup.error('Failed to add income')
          });
      }
    });
  }

  protected deleteIncome(id: number): void {
    this.incomeService
      .delete(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => { this.popup.success('Income deleted'); this.capitalService.refresh(); },
        error: () => this.popup.error('Failed to delete income')
      });
  }

  private rebuild(): void {
    this.orderedGroups.set(this.applySort(this.groupByCategory()));
  }

  private groupByCategory(): BreakdownGroup[] {
    const total = this.total();
    const byCategory = new Map<number, BreakdownGroup>();

    for (const income of this.incomes()) {
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
      group.items.push(income);
      byCategory.set(income.categoryId, group);
    }

    return [...byCategory.values()].map(group => ({ ...group, percentage: total > 0 ? group.total / total * 100 : null }));
  }

  private applySort(data: BreakdownGroup[]): BreakdownGroup[] {
    const sorted = [...data];

    switch (this.sortBy()) {
      case 'amount-asc': return sorted.sort((a, b) => a.total - b.total);
      case 'name': return sorted.sort((a, b) => a.name.localeCompare(b.name));
      case 'custom': return sorted;
      default: return sorted.sort((a, b) => b.total - a.total);
    }
  }
}
