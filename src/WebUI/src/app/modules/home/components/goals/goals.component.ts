import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { SharedModule } from '../../shared/shared.module';
import { Goal } from '../home/models/goal.model';
import { GoalService, CreateGoalRequest, UpdateGoalRequest } from '../home/services/goal.service';
import { GoalDialogComponent, GoalDialogData } from '../home/components/goal-dialog/goal-dialog.component';
import { DialogService } from '../../shared/components/dialogs/services/dialog.service';
import { ConfirmDialogComponent } from '../../shared/components/dialogs/confirm-dialog/confirm-dialog.component';
import { PopupMessageService } from '../../shared/services/popup-message.service';
import {SharedModule} from "../../../../shared/shared.module";

@Component({
  selector: 'app-goals',
  templateUrl: './goals.component.html',
  styleUrl: './goals.component.scss',
  standalone: true,
  imports: [SharedModule, SharedModule]
})
export class GoalsComponent implements OnInit, OnDestroy {
  goals: Goal[] = [];
  loading = false;

  private unsubscribe$ = new Subject<void>();

  constructor(
    private readonly goalService: GoalService,
    private readonly dialogService: DialogService,
    private readonly popup: PopupMessageService
  ) {}

  goalProgress(goal: Goal): number {
    if (!goal.targetAmount) return 0;
    return Math.min(100, Math.round((goal.currentAmount / goal.targetAmount) * 100));
  }

  get activeGoals(): Goal[] {
    return this.goals.filter(g => !g.isCompleted);
  }

  get completedGoals(): Goal[] {
    return this.goals.filter(g => g.isCompleted);
  }

  openDialog(goal?: Goal): void {
    const data: GoalDialogData = { goal };
    const ref = this.dialogService.open(GoalDialogComponent, { data });
    ref.afterClosed$.pipe(takeUntil(this.unsubscribe$)).subscribe(result => {
      if (!result) return;
      if (goal) {
        const req: UpdateGoalRequest = { ...result, isCompleted: result.isCompleted ?? goal.isCompleted };
        this.goalService.update(goal.id, req).subscribe({
          next: () => { this.popup.success('Goal updated'); this.load(); },
          error: () => this.popup.error('Failed to update goal')
        });
      } else {
        const req: CreateGoalRequest = result;
        this.goalService.create(req).subscribe({
          next: () => { this.popup.success('Goal added'); this.load(); },
          error: () => this.popup.error('Failed to add goal')
        });
      }
    });
  }

  confirmDelete(goal: Goal): void {
    const ref = this.dialogService.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Goal',
        message: `Delete "${goal.title}"? This cannot be undone.`,
        submitText: 'Delete',
        cancelText: 'Cancel',
        icon: 'warning'
      }
    });
    ref.afterClosed$.pipe(takeUntil(this.unsubscribe$)).subscribe(confirmed => {
      if (!confirmed) return;
      this.goalService.delete(goal.id).subscribe({
        next: () => { this.goals = this.goals.filter(g => g.id !== goal.id); this.popup.success('Goal deleted'); },
        error: () => this.popup.error('Failed to delete goal')
      });
    });
  }

  ngOnInit(): void {
    document.title = 'Deed - Goals';
    this.load();
  }

  private load(): void {
    this.loading = true;
    this.goalService.getAll().pipe(takeUntil(this.unsubscribe$)).subscribe({
      next: goals => { this.goals = goals; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  ngOnDestroy(): void {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }
}
