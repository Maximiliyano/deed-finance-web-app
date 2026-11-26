import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { EMPTY, Observable, catchError } from 'rxjs';
import { CURRENCIES } from '../../../../core/types/currency-type';
import { ConfirmDialogComponent } from '../../../../shared/components/dialogs/confirm-dialog/confirm-dialog.component';
import { DialogService } from '../../../../shared/components/dialogs/services/dialog.service';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { PanelStateComponent } from '../../../../shared/components/panel-state/panel-state.component';
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
import { CapitalCurrencyService } from '../../../capital/services/capital-currency.service';
import { DashboardStore } from '../../services/dashboard-store.service';
import { DebtService } from '../../services/debt.service';
import { UserSettingsService } from '../../services/user-settings.service';
import { QuickAddService } from '../../services/quick-add.service';

export type CapitalSort = 'custom' | 'balance-desc' | 'balance-asc' | 'name' | 'incomes' | 'expenses';
export type CapitalFilter = 'all' | 'savings' | 'excluded';

@Component({
  selector: 'app-capitals-panel',
  templateUrl: './capitals-panel.component.html',
  styleUrl: './capitals-panel.component.scss',
  standalone: true,
  imports: [SharedModule, DragDropModule, SortToolbarComponent, PanelStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CapitalsPanelComponent implements OnInit {
  protected readonly store = inject(DashboardStore);
  private readonly capitalService = inject(CapitalService);
  private readonly transferService = inject(TransferService);
  private readonly quickAdd = inject(QuickAddService);
  private readonly debtService = inject(DebtService);
  private readonly userSettingsService = inject(UserSettingsService);
  private readonly dialogService = inject(DialogService);
  private readonly popup = inject(PopupMessageService);
  private readonly capitalCurrency = inject(CapitalCurrencyService);
  private readonly sectionLoading = inject(SectionLoadingService);
  private readonly destroyRef = inject(DestroyRef);

  readonly isAnonymous = this.store.isAnonymous;
  readonly userCurrency = this.store.userCurrency;
  readonly currencyOptions = CURRENCIES;
  readonly sortOptions: SortOption[] = [
    { key: 'custom', title: 'Custom order', icon: 'fa-grip-vertical' },
    { key: 'balance-desc', title: 'Highest balance', icon: 'fa-arrow-down-wide-short' },
    { key: 'balance-asc', title: 'Lowest balance', icon: 'fa-arrow-up-short-wide' },
    { key: 'name', title: 'A to Z', icon: 'fa-arrow-down-a-z' },
    { key: 'incomes', title: 'Most income', icon: 'fa-arrow-trend-up' },
    { key: 'expenses', title: 'Most spent', icon: 'fa-arrow-trend-down' }
  ];

  readonly sortBy = signal<CapitalSort>('custom');
  readonly filterBy = signal<CapitalFilter>('all');
  readonly searchTerm = signal('');
  readonly expandedCapitalId = signal<number | null>(null);
  readonly loading = toSignal(this.sectionLoading.isLoading$('capitals'), { initialValue: false });

  private readonly debts = toSignal(this.debtService.debts$, { initialValue: [] });
  private readonly transfers = toSignal(this.transferService.transfers$, { initialValue: [] });
  private transfersRequested = false;

  readonly hasFilters = computed(() => this.searchTerm().trim() !== '' || this.filterBy() !== 'all');

  readonly capitals = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const filter = this.filterBy();

    const items = this.store
      .capitals()
      .filter(c => !term || c.name.toLowerCase().includes(term))
      .filter(c => {
        switch (filter) {
          case 'savings':
            return c.onlyForSavings;
          case 'excluded':
            return !c.includeInTotal;
          default:
            return true;
        }
      });

    switch (this.sortBy()) {
      case 'balance-desc':
        return items.sort((a, b) => b.balance - a.balance);
      case 'balance-asc':
        return items.sort((a, b) => a.balance - b.balance);
      case 'name':
        return items.sort((a, b) => a.name.localeCompare(b.name));
      case 'incomes':
        return items.sort((a, b) => b.totalIncome - a.totalIncome);
      case 'expenses':
        return items.sort((a, b) => b.totalExpense - a.totalExpense);
      default:
        return items;
    }
  });

  readonly dragDisabled = computed(() => this.sortBy() !== 'custom' || this.hasFilters() || this.isAnonymous());

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

    return id == null ? [] : this.transfers().filter(t => t.sourceCapitalId === id || t.destinationCapitalId === id);
  });

  ngOnInit(): void {
    this.capitalService
      .load({ searchTerm: null, sortBy: null, sortDirection: null, filterBy: null })
      .pipe(
        catchError(() => EMPTY),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe();

    this.debtService
      .load()
      .pipe(
        catchError(() => EMPTY),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe();
  }

  sortCapitals(by: string): void {
    this.sortBy.set(by as CapitalSort);
  }

  setSearch(event: Event): void {
    this.searchTerm.set((event.target as HTMLInputElement).value);
  }

  setFilter(event: Event): void {
    this.filterBy.set((event.target as HTMLSelectElement).value as CapitalFilter);
  }

  clearFilters(): void {
    this.searchTerm.set('');
    this.filterBy.set('all');
  }

  changeCurrency(event: Event): void {
    const currency = (event.target as HTMLSelectElement).value;
    if (!currency || currency === this.userCurrency()) return;

    this.userSettingsService
      .patch({ currency })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.popup.success(`Totals are now shown in ${currency}`),
        error: () => this.popup.error('Failed to change currency')
      });
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
      this.transferService
        .load()
        .pipe(
          catchError(() => EMPTY),
          takeUntilDestroyed(this.destroyRef)
        )
        .subscribe();
    }
  }

  openCapitalDetail(cap: CapitalResponse): void {
    const ref = this.dialogService.open(CapitalDetailsComponent, {
      data: { capital: cap }
    });

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((result: UpdateCapitalRequest | null) => {
      if (!result) return;

      this.capitalService
        .update(result.id, result)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.popup.success('Capital updated');
            if (result.currency) this.capitalCurrency.offerBudgetRecalculation(result.id, result.currency);
          },
          error: () => this.popup.error('Failed to update capital')
        });
    });
  }

  openAddCapital(): void {
    const ref = this.dialogService.open(AddCapitalDialogComponent);

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((result: AddCapitalRequest | null) => {
      if (!result) return;

      this.capitalService
        .create(result)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => this.popup.success('Capital added'),
          error: () => this.popup.error('Failed to add capital')
        });
    });
  }

  toggleIncludeInTotal(cap: CapitalResponse): void {
    const include = !cap.includeInTotal;
    this.patch(
      this.capitalService.patchIncludeTotal(cap.id, include),
      `${cap.name} ${include ? 'included in' : 'excluded from'} the total`
    );
  }

  toggleSavingsOnly(cap: CapitalResponse): void {
    const savings = !cap.onlyForSavings;
    this.patch(
      this.capitalService.patchSavingsOnly(cap.id, savings),
      `${cap.name} is now ${savings ? 'savings only' : 'a regular capital'}`
    );
  }

  deleteCapital(cap: CapitalResponse): void {
    const ref = this.dialogService.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete capital',
        message: `Delete "${cap.name}"? A capital that still has expenses, incomes, transfers, bills or debts cannot be deleted.`,
        icon: 'danger'
      }
    });

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((confirmed: boolean) => {
      if (!confirmed) return;

      this.capitalService
        .delete(cap.id)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => this.popup.success(`${cap.name} deleted`),
          error: () => this.popup.error('This capital could not be deleted. It may still have records linked to it.')
        });
    });
  }

  openTransferDialog(sourceCapitalId?: number): void {
    void this.quickAdd.transfer(sourceCapitalId);
  }

  deleteTransfer(id: number): void {
    this.transferService
      .delete(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.popup.success('Transfer reversed and deleted'),
        error: () => this.popup.error('Failed to delete transfer')
      });
  }

  private patch(request: Observable<unknown>, message: string): void {
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => this.popup.success(message),
      error: () => this.popup.error('Failed to update capital')
    });
  }
}
