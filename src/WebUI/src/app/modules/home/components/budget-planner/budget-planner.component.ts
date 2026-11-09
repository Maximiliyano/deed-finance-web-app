import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, of, switchMap } from 'rxjs';
import { DialogService } from '../../../../shared/components/dialogs/services/dialog.service';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SectionLoadingService } from '../../../../shared/services/section-loading.service';
import { NAMED_AMOUNT_SORT_OPTIONS, SortToolbarComponent } from '../../../../shared/components/sort-toolbar/sort-toolbar.component';
import { SharedModule } from '../../../../shared/shared.module';
import { convertCurrency } from '../../../../shared/utils/currency-conversion.util';
import { BudgetEstimation } from '../../models/budget-estimation';
import { BudgetEstimationForm } from '../../models/budget-estimation-form';
import { CreateBudgetEstimationRequest } from '../../models/budget-estimation-request-create';
import { UpdateBudgetEstimationRequest } from '../../models/budget-estimation-request-update';
import { BudgetEstimationService } from '../../services/budget-estimation.service';
import { DashboardStore } from '../../services/dashboard-store.service';
import {
  BudgetEstimationDialogComponent,
  BudgetEstimationDialogData
} from '../budget-estimation-dialog/budget-estimation-dialog.component';
import { BudgetChartsComponent } from './budget-charts/budget-charts.component';
import { BUDGET_BAR_COLORS } from './budget-planner.constants';
import { BudgetPeriodBarComponent } from './budget-period-bar/budget-period-bar.component';

type EstimationSort = 'custom' | 'amount-asc' | 'amount-desc' | 'name';

function sortEstimations(items: readonly BudgetEstimation[], by: EstimationSort): BudgetEstimation[] {
  const sorted = [...items];

  switch (by) {
    case 'amount-desc': return sorted.sort((a, b) => b.budgetAmount - a.budgetAmount);
    case 'amount-asc': return sorted.sort((a, b) => a.budgetAmount - b.budgetAmount);
    case 'name': return sorted.sort((a, b) => a.description.localeCompare(b.description));
    default: return sorted.sort((a, b) => a.orderIndex - b.orderIndex);
  }
}

@Component({
  selector: 'app-budget-planner',
  templateUrl: './budget-planner.component.html',
  styleUrl: './budget-planner.component.scss',
  standalone: true,
  imports: [SharedModule, DragDropModule, BudgetPeriodBarComponent, BudgetChartsComponent, SortToolbarComponent],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BudgetPlannerComponent implements OnInit {
  protected readonly store = inject(DashboardStore);
  private readonly budgetEstimationService = inject(BudgetEstimationService);
  private readonly dialogService = inject(DialogService);
  private readonly popup = inject(PopupMessageService);
  private readonly sectionLoading = inject(SectionLoadingService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly barColors = BUDGET_BAR_COLORS;
  protected readonly showCharts = signal(false);
  protected readonly sortOptions = NAMED_AMOUNT_SORT_OPTIONS;
  protected readonly sortBy = signal<EstimationSort>('custom');
  protected readonly loaded = signal(false);
  protected readonly loading = toSignal(this.sectionLoading.isLoading$('budgetEstimations'), { initialValue: false });

  private readonly estimations = toSignal(this.budgetEstimationService.estimations$, { initialValue: [] });
  private readonly period$ = toObservable(this.store.period);

  protected readonly sortedEstimations = computed(() => sortEstimations(this.estimations(), this.sortBy()));
  protected setSort(by: string): void {
    this.sortBy.set(by as EstimationSort);
  }

  protected readonly isDragDisabled = computed(() => this.sortBy() !== 'custom' || this.store.isAnonymous());
  protected readonly isProfitable = computed(() => this.store.profit() >= 0);

  ngOnInit(): void {
    this.period$
      .pipe(
        switchMap(period => this.budgetEstimationService.load(period).pipe(catchError(() => of([])))),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.loaded.set(true));
  }

  protected convertUserCurrency(amount: number, from: string): number {
    return convertCurrency(amount, from, this.store.userCurrency(), this.store.exchanges());
  }

  protected budgetPercent(amount: number, fromCurrency?: string): number {
    const totalCapital = this.store.totalCapital();

    if (totalCapital <= 0) return 0;

    const converted = fromCurrency ? this.convertUserCurrency(amount, fromCurrency) : amount;

    return Math.min(100, Math.round((converted / totalCapital) * 100));
  }

  protected toggleCharts(): void {
    this.showCharts.update(value => !value);
  }

  protected dropEstimation(event: CdkDragDrop<BudgetEstimation[]>): void {
    if (event.previousIndex === event.currentIndex) return;

    const moved = [...this.sortedEstimations()];
    moveItemInArray(moved, event.previousIndex, event.currentIndex);

    this.budgetEstimationService
      .updateOrder(moved.map((estimation, index) => ({ id: estimation.id, orderIndex: index })))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();
  }

  protected openEstimationDialog(estimation?: BudgetEstimation): void {
    const data: BudgetEstimationDialogData = { estimation, capitals: this.store.capitals() };
    const ref = this.dialogService.open(BudgetEstimationDialogComponent, { data });

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((result: BudgetEstimationForm | null) => {
      if (!result) return;

      if (estimation) {
        const request: UpdateBudgetEstimationRequest = result;

        this.budgetEstimationService.update(estimation.id, request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
          next: () => this.popup.success('Estimation updated'),
          error: () => this.popup.error('Failed to update estimation')
        });
      } else {
        const request: CreateBudgetEstimationRequest = result;

        this.budgetEstimationService.create(request, result.capital ?? null).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
          next: () => this.popup.success('Estimation added'),
          error: () => this.popup.error('Failed to add estimation')
        });
      }
    });
  }

  protected copyEstimation(source: BudgetEstimation): void {
    const data: BudgetEstimationDialogData = { estimation: source, capitals: this.store.capitals(), mode: 'copy' };
    const ref = this.dialogService.open(BudgetEstimationDialogComponent, { data });

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(result => {
      if (!result) return;

      const request: CreateBudgetEstimationRequest = result;

      this.budgetEstimationService.create(request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => this.popup.success('Estimation copied'),
        error: () => this.popup.error('Failed to copy estimation')
      });
    });
  }

  protected deleteEstimation(estimation: BudgetEstimation): void {
    this.budgetEstimationService.delete(estimation.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => this.popup.success('Estimation deleted'),
      error: () => this.popup.error('Failed to delete estimation')
    });
  }

  protected toggleEstimationCompleted(estimation: BudgetEstimation, event: Event): void {
    event.stopPropagation();

    const request: UpdateBudgetEstimationRequest = {
      description: estimation.description,
      budgetAmount: estimation.budgetAmount,
      budgetCurrency: estimation.budgetCurrency,
      capitalId: estimation.capitalId,
      isCompleted: (event.target as HTMLInputElement).checked
    };

    this.budgetEstimationService.update(estimation.id, request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => this.popup.success('Estimation updated'),
      error: () => this.popup.error('Failed to update estimation')
    });
  }
}
