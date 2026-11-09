import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { EMPTY, catchError, finalize } from 'rxjs';
import { DialogService } from '../../../../shared/components/dialogs/services/dialog.service';
import { SortOption, SortToolbarComponent } from '../../../../shared/components/sort-toolbar/sort-toolbar.component';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SectionLoadingService } from '../../../../shared/services/section-loading.service';
import { SharedModule } from '../../../../shared/shared.module';
import { Debt } from '../../models/debt.model';
import { DebtForm } from '../../models/debt-form';
import { CreateDebtRequest } from '../../models/debt-request-create';
import { UpdateDebtRequest } from '../../models/debt-request-update';
import { DashboardStore } from '../../services/dashboard-store.service';
import { DebtService } from '../../services/debt.service';
import { DebtDialogComponent, DebtDialogData } from '../debt-dialog/debt-dialog.component';

type DebtSort = 'custom' | 'amount-desc' | 'amount-asc' | 'name';
type DebtStatus = 'overdue' | 'soon' | 'ok';

@Component({
  selector: 'app-debts-panel',
  templateUrl: './debts-panel.component.html',
  styleUrl: './debts-panel.component.scss',
  standalone: true,
  imports: [SharedModule, DragDropModule, SortToolbarComponent],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DebtsPanelComponent implements OnInit {
  readonly store = inject(DashboardStore);
  private readonly debtService = inject(DebtService);
  private readonly dialogService = inject(DialogService);
  private readonly popup = inject(PopupMessageService);
  private readonly sectionLoading = inject(SectionLoadingService);
  private readonly destroyRef = inject(DestroyRef);

  readonly sortOptions: SortOption[] = [
    { key: 'custom', title: 'Custom order', icon: 'fa-grip-vertical' },
    { key: 'amount-desc', title: 'Highest first', icon: 'fa-arrow-down-wide-short' },
    { key: 'amount-asc', title: 'Lowest first', icon: 'fa-arrow-up-short-wide' },
    { key: 'name', title: 'A to Z', icon: 'fa-arrow-down-a-z' }
  ];

  readonly sortBy = signal<DebtSort>('custom');
  private readonly loaded = signal(false);
  private readonly requestPending = toSignal(this.sectionLoading.isLoading$('debts'), { initialValue: false });
  private readonly storeDebts = toSignal(this.debtService.debts$, { initialValue: [] });

  readonly isLoading = computed(() => !this.loaded() || this.requestPending());

  readonly debts = computed(() => {
    const debts = [...this.storeDebts()];

    switch (this.sortBy()) {
      case 'amount-desc': return debts.sort((a, b) => b.amount - a.amount);
      case 'amount-asc': return debts.sort((a, b) => a.amount - b.amount);
      case 'name': return debts.sort((a, b) => a.item.localeCompare(b.item));
      default: return debts.sort((a, b) => a.orderIndex - b.orderIndex);
    }
  });

  readonly statuses = computed(() => new Map(this.debts().map(debt => [debt.id, this.statusOf(debt)])));

  ngOnInit(): void {
    this.debtService.load()
      .pipe(
        catchError(() => EMPTY),
        finalize(() => this.loaded.set(true)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe();
  }

  setSort(by: string): void {
    this.sortBy.set(by as DebtSort);
  }

  drop(event: CdkDragDrop<Debt[]>): void {
    if (event.previousIndex === event.currentIndex) return;

    const moved = [...this.debts()];
    moveItemInArray(moved, event.previousIndex, event.currentIndex);
    this.debtService.updateOrder(moved.map((debt, index) => ({ id: debt.id, orderIndex: index })))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();
  }

  openDialog(debt?: Debt): void {
    const data: DebtDialogData = { debt, capitals: this.store.capitals() };
    const ref = this.dialogService.open(DebtDialogComponent, { data });

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((result: DebtForm | null) => {
      if (!result) return;

      if (debt) {
        const request: UpdateDebtRequest = {
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
        this.debtService.update(debt.id, request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
          next: () => this.popup.success('Debt updated'),
          error: () => this.popup.error('Failed to update debt')
        });
      } else {
        const request: CreateDebtRequest = {
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
        this.debtService.create(request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
          next: () => this.popup.success('Debt added'),
          error: () => this.popup.error('Failed to add debt')
        });
      }
    });
  }

  delete(debt: Debt): void {
    this.debtService.delete(debt.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => this.popup.success('Debt deleted'),
      error: () => this.popup.error('Failed to delete debt')
    });
  }

  private statusOf(debt: Debt): DebtStatus {
    if (!debt.deadlineAt) return 'ok';

    const diffDays = Math.ceil((new Date(debt.deadlineAt).getTime() - Date.now()) / 86400000);
    if (diffDays < 0) return 'overdue';
    if (diffDays <= 7) return 'soon';
    return 'ok';
  }
}
