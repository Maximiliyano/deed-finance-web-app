import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { EMPTY, catchError, finalize } from 'rxjs';
import { PanelStateComponent } from '../../../../shared/components/panel-state/panel-state.component';
import { DialogService } from '../../../../shared/components/dialogs/services/dialog.service';
import { SortOption, SortToolbarComponent } from '../../../../shared/components/sort-toolbar/sort-toolbar.component';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SectionLoadingService } from '../../../../shared/services/section-loading.service';
import { SharedModule } from '../../../../shared/shared.module';
import { convertCurrency } from '../../../../shared/utils/currency-conversion.util';
import { Goal } from '../../models/goal.model';
import { DashboardStore } from '../../services/dashboard-store.service';
import { GoalService, UpdateGoalRequest } from '../../services/goal.service';
import { GoalDialogComponent, GoalDialogData } from '../goal-dialog/goal-dialog.component';
import { allocateGoal, summarizeBudget, toTip } from './goal-allocation';

type GoalSort = 'custom' | 'progress-desc' | 'progress-asc' | 'name';

@Component({
  selector: 'app-goals-panel',
  templateUrl: './goals-panel.component.html',
  styleUrl: './goals-panel.component.scss',
  standalone: true,
  imports: [SharedModule, DragDropModule, SortToolbarComponent, PanelStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GoalsPanelComponent implements OnInit {
  readonly store = inject(DashboardStore);
  private readonly goalService = inject(GoalService);
  private readonly dialogService = inject(DialogService);
  private readonly popup = inject(PopupMessageService);
  private readonly sectionLoading = inject(SectionLoadingService);
  private readonly destroyRef = inject(DestroyRef);

  readonly sortOptions: SortOption[] = [
    { key: 'custom', title: 'Custom order', icon: 'fa-grip-vertical' },
    { key: 'progress-desc', title: 'Most progress', icon: 'fa-arrow-down-wide-short' },
    { key: 'progress-asc', title: 'Least progress', icon: 'fa-arrow-up-short-wide' },
    { key: 'name', title: 'A to Z', icon: 'fa-arrow-down-a-z' }
  ];

  readonly sortBy = signal<GoalSort>('custom');
  private readonly loaded = signal(false);
  private readonly requestPending = toSignal(this.sectionLoading.isLoading$('goals'), { initialValue: false });
  private readonly storeGoals = toSignal(this.goalService.goals$, { initialValue: [] });

  readonly isLoading = computed(() => !this.loaded() || this.requestPending());

  readonly goals = computed(() => {
    const goals = [...this.storeGoals()];

    switch (this.sortBy()) {
      case 'progress-desc':
        return goals.sort((a, b) => this.goalProgress(b) - this.goalProgress(a));
      case 'progress-asc':
        return goals.sort((a, b) => this.goalProgress(a) - this.goalProgress(b));
      case 'name':
        return goals.sort((a, b) => a.title.localeCompare(b.title));
      default:
        return goals.sort((a, b) => a.orderIndex - b.orderIndex);
    }
  });

  private readonly remainingOf = (goal: Goal): number =>
    convertCurrency(
      goal.targetAmount - goal.currentAmount,
      goal.currency,
      this.store.userCurrency(),
      this.store.exchanges()
    );

  readonly budgetSummary = computed(() => summarizeBudget(this.goals(), this.store.profit(), this.remainingOf));

  readonly tips = computed(() => {
    const summary = this.budgetSummary();
    const tips = new Map<number, ReturnType<typeof toTip>>();

    for (const goal of this.goals()) {
      const allocation = allocateGoal(goal, summary, this.remainingOf);
      if (allocation) tips.set(goal.id, toTip(allocation));
    }

    return tips;
  });

  ngOnInit(): void {
    this.goalService
      .load()
      .pipe(
        catchError(() => EMPTY),
        finalize(() => this.loaded.set(true)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe();
  }

  goalProgress(goal: Goal): number {
    if (!goal.targetAmount) return 0;
    return Math.min(100, Math.round((goal.currentAmount / goal.targetAmount) * 100));
  }

  setSort(by: string): void {
    this.sortBy.set(by as GoalSort);
  }

  drop(event: CdkDragDrop<Goal[]>): void {
    if (event.previousIndex === event.currentIndex) return;

    const moved = [...this.goals()];
    moveItemInArray(moved, event.previousIndex, event.currentIndex);
    this.goalService
      .updateOrder(moved.map((goal, index) => ({ id: goal.id, orderIndex: index })))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();
  }

  openDialog(goal?: Goal): void {
    const data: GoalDialogData = { goal };
    const ref = this.dialogService.open(GoalDialogComponent, { data });

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(result => {
      if (!result) return;

      if (goal) {
        const request: UpdateGoalRequest = { ...result, isCompleted: result.isCompleted ?? goal.isCompleted };
        this.goalService
          .update(goal.id, request)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => this.popup.success('Goal updated'),
            error: () => this.popup.error('Failed to update goal')
          });
      } else {
        this.goalService
          .create(result)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => this.popup.success('Goal added'),
            error: () => this.popup.error('Failed to add goal')
          });
      }
    });
  }

  delete(goal: Goal): void {
    this.goalService
      .delete(goal.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.popup.success('Goal deleted'),
        error: () => this.popup.error('Failed to delete goal')
      });
  }
}
