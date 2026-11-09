import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { EMPTY, catchError } from 'rxjs';
import { DialogService } from '../../../../shared/components/dialogs/services/dialog.service';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SectionLoadingService } from '../../../../shared/services/section-loading.service';
import { TransferService } from '../../../../shared/services/transfer.service';
import { SortOption, SortToolbarComponent } from '../../../../shared/components/sort-toolbar/sort-toolbar.component';
import { SharedModule } from '../../../../shared/shared.module';
import { CapitalDetailsComponent } from '../../../capital/components/capital-details/capital-details.component';
import { AddCapitalDialogComponent } from '../../../capital/components/capital-dialog/add-capital-dialog.component';
import { AddCapitalRequest } from '../../../capital/models/add-capital-request';
import { CapitalResponse } from '../../../capital/models/capital-response';
import { UpdateCapitalRequest } from '../../../capital/models/update-capital-request';
import { CapitalService } from '../../../capital/services/capital.service';
import { DashboardStore } from '../../services/dashboard-store.service';
import { DebtService } from '../../services/debt.service';
import { TransferDialogComponent, TransferDialogData } from '../transfer-dialog/transfer-dialog.component';

export type CapitalSort = 'custom' | 'balance-desc' | 'balance-asc' | 'name';

@Component({
  selector: 'app-capitals-panel',
  templateUrl: './capitals-panel.component.html',
  styleUrl: './capitals-panel.component.scss',
  standalone: true,
  imports: [SharedModule, RouterLink, DragDropModule, SortToolbarComponent],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CapitalsPanelComponent implements OnInit {
  private readonly store = inject(DashboardStore);
  private readonly capitalService = inject(CapitalService);
  private readonly transferService = inject(TransferService);
  private readonly debtService = inject(DebtService);
  private readonly dialogService = inject(DialogService);
  private readonly popup = inject(PopupMessageService);
  private readonly sectionLoading = inject(SectionLoadingService);
  private readonly destroyRef = inject(DestroyRef);

  readonly isAnonymous = this.store.isAnonymous;
  readonly sortOptions: SortOption[] = [
    { key: 'custom', title: 'Custom order', icon: 'fa-grip-vertical' },
    { key: 'balance-desc', title: 'Highest balance', icon: 'fa-arrow-down-wide-short' },
    { key: 'balance-asc', title: 'Lowest balance', icon: 'fa-arrow-up-short-wide' },
    { key: 'name', title: 'A to Z', icon: 'fa-arrow-down-a-z' }
  ];

  readonly sortBy = signal<CapitalSort>('custom');
  readonly expandedCapitalId = signal<number | null>(null);
  readonly loading = toSignal(this.sectionLoading.isLoading$('capitals'), { initialValue: false });

  private readonly debts = toSignal(this.debtService.debts$, { initialValue: [] });
  private readonly transfers = toSignal(this.transferService.transfers$, { initialValue: [] });
  private transfersRequested = false;

  readonly capitals = computed(() => {
    const items = [...this.store.capitals()];

    switch (this.sortBy()) {
      case 'balance-desc': return items.sort((a, b) => b.balance - a.balance);
      case 'balance-asc': return items.sort((a, b) => a.balance - b.balance);
      case 'name': return items.sort((a, b) => a.name.localeCompare(b.name));
      default: return items;
    }
  });

  readonly dragDisabled = computed(() => this.sortBy() !== 'custom' || this.isAnonymous());

  readonly borrowedByCapital = computed(() => {
    const borrowed = new Map<number, number>();

    for (const debt of this.debts()) {
      if (debt.capitalId != null && !debt.isPaid) {
        borrowed.set(debt.capitalId, (borrowed.get(debt.capitalId) ?? 0) + debt.amount);
      }
    }

    return borrowed;
  });

  readonly expandedTransfers = computed(() => {
    const id = this.expandedCapitalId();

    return id == null
      ? []
      : this.transfers().filter(t => t.sourceCapitalId === id || t.destinationCapitalId === id);
  });

  ngOnInit(): void {
    this.capitalService
      .load({ searchTerm: null, sortBy: null, sortDirection: null, filterBy: null })
      .pipe(catchError(() => EMPTY), takeUntilDestroyed(this.destroyRef))
      .subscribe();

    this.debtService.load()
      .pipe(catchError(() => EMPTY), takeUntilDestroyed(this.destroyRef))
      .subscribe();
  }

  sortCapitals(by: string): void {
    this.sortBy.set(by as CapitalSort);
  }

  dropCapital(event: CdkDragDrop<CapitalResponse[]>): void {
    if (event.previousIndex === event.currentIndex) return;

    const moved = [...this.capitals()];
    moveItemInArray(moved, event.previousIndex, event.currentIndex);
    const orders = moved.map((c, i) => ({ id: c.id, orderIndex: i }));

    this.capitalService.updateOrder({ capitals: orders }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
  }

  showTransfers(cap: CapitalResponse): void {
    const expanding = this.expandedCapitalId() !== cap.id;
    this.expandedCapitalId.set(expanding ? cap.id : null);

    if (expanding && !this.transfersRequested) {
      this.transfersRequested = true;
      this.transferService.load()
        .pipe(catchError(() => EMPTY), takeUntilDestroyed(this.destroyRef))
        .subscribe();
    }
  }

  openCapitalDetail(cap: CapitalResponse): void {
    const ref = this.dialogService.open(CapitalDetailsComponent, {
      data: { capital: cap }
    });

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((result: UpdateCapitalRequest | null) => {
      if (!result) return;

      this.capitalService.update(result.id, result).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => this.popup.success('Capital updated'),
        error: () => this.popup.error('Failed to update capital')
      });
    });
  }

  openAddCapital(): void {
    const ref = this.dialogService.open(AddCapitalDialogComponent);

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((result: AddCapitalRequest | null) => {
      if (!result) return;

      this.capitalService.create(result).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => this.popup.success('Capital added'),
        error: () => this.popup.error('Failed to add capital')
      });
    });
  }

  openTransferDialog(sourceCapitalId?: number): void {
    const data: TransferDialogData = {
      capitals: this.store.capitals(),
      exchanges: this.store.exchanges(),
      sourceCapitalId: sourceCapitalId ?? null
    };
    const ref = this.dialogService.open(TransferDialogComponent, { data });

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(result => {
      if (!result) return;

      this.transferService.create(result).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => this.popup.success('Transfer completed'),
        error: () => this.popup.error('Failed to create transfer')
      });
    });
  }

  deleteTransfer(id: number): void {
    this.transferService.delete(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => this.popup.success('Transfer reversed and deleted'),
      error: () => this.popup.error('Failed to delete transfer')
    });
  }
}
