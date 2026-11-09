import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, Subject } from 'rxjs';
import { Exchange } from '../../core/models/exchange-model';
import { DialogService } from '../../shared/components/dialogs/services/dialog.service';
import { ExchangeService } from '../../shared/services/exchange.service';
import { PopupMessageService } from '../../shared/services/popup-message.service';
import { AddCapitalRequest } from './models/add-capital-request';
import { CapitalItem } from './models/capital-item';
import { CapitalResponse } from './models/capital-response';
import { CapitalService } from './services/capital.service';
import { UpdateCapitalRequest } from './models/update-capital-request';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { AddCapitalDialogComponent } from './components/capital-dialog/add-capital-dialog.component';
import { CapitalDetailsComponent } from './components/capital-details/capital-details.component';
import { ConfirmDialogComponent } from '../../shared/components/dialogs/confirm-dialog/confirm-dialog.component';
import { User } from '../auth/models/user';
import { AuthService } from '../auth/services/auth-service';
import { Debt } from '../home/models/debt.model';
import { DebtService } from '../home/services/debt.service';
import { CURRENCIES, CURRENCY_DEFAULT } from '../../core/types/currency-type';
import { UserSettingsService } from '../home/services/user-settings.service';
import { convertCurrency } from '../../shared/utils/currency-conversion.util';

@Component({
    selector: 'app-capitals',
    templateUrl: './capitals.component.html',
    styleUrl: './capitals.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: false
})
export class CapitalsComponent implements OnInit, OnDestroy {
  private readonly capitalService = inject(CapitalService);
  private readonly popupMessageService = inject(PopupMessageService);
  private readonly exchangeService = inject(ExchangeService);
  private readonly dialogService = inject(DialogService);
  private readonly authService = inject(AuthService);
  private readonly debtService = inject(DebtService);
  private readonly userSettingsService = inject(UserSettingsService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  capitals: CapitalResponse[] = [];
  capitalStatItems: CapitalItem[] = [
    { key: 'totalIncome', title: 'Incomes', icon: 'fa-dollar-sign', style: 'cp-incomes' },
    { key: 'totalExpense', title: 'Expenses', icon: 'fa-money-bill-wave', style: 'cp-expenses', url: '/expenses' },
    { key: 'totalTransferOut', title: 'Transfer Out', icon: 'fa-arrow-up', style: 'cp-transfer-out' },
    { key: 'totalTransferIn', title: 'Transfer In', icon: 'fa-arrow-down', style: 'cp-transfer-in' },
  ];
  exchanges: Exchange[] = [];
  debts: Debt[] = [];

  searchTerm = '';
  selectedSortOption: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';
  filterBy: 'onlyForSavings' | null = null;

  mainCurrency: string;

  sortOptions: {label: string; key: string}[] = [
    { label: 'custom', key: '' },
    { label: 'name', key: 'name' },
    { label: 'balance', key: 'balance' },
    { label: 'expenses', key: 'totalExpense' },
    { label: 'incomes', key: 'totalIncome' },
    { label: 'transfers in', key: 'totalTransferIn' },
    { label: 'transfers out', key: 'totalTransferOut' },
  ];

  currencyOptions = CURRENCIES;

  user: User | null = null;

  private queryParams$ = new Subject<void>();
  ngOnInit(): void {
    document.title = "Deed - Capitals";

    this.authService.user$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(user => {
        this.user = user;
        this.cdr.markForCheck();
      });

    this.mainCurrency = this.user?.currency ?? CURRENCY_DEFAULT;

    this.queryParams$
      .pipe(
        debounceTime(300),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () => this.fetchCapitals()
      })

    this.fetchCapitals();
    this.fetchExchanges();
    this.debtService.load().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: debts => { this.debts = debts; this.cdr.markForCheck(); }
    });
  }

  borrowedAmount(capitalId: number): number {
    return this.debts
      .filter(d => d.capitalId === capitalId && !d.isPaid)
      .reduce((sum, d) => sum + d.amount, 0);
  }

  ngOnDestroy(): void {
    this.queryParams$.complete();
  }

  fetchExchanges(): void {
    this.exchangeService
      .getLatest()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => { this.exchanges = response; this.cdr.markForCheck(); },
        error: () => this.popupMessageService.error('Failed to load exchanges')
      });
  }

  fetchCapitals(): void {
    this.capitalService.load({
        searchTerm: this.searchTerm,
        sortBy: this.selectedSortOption,
        sortDirection: this.sortDirection,
        filterBy: this.filterBy
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.capitals = response;
          this.cdr.markForCheck();
        },
        error: () => this.popupMessageService.error('Failed to load capitals')
    });
  }

  onFilterChange(): void {
    this.queryParams$.next();
  }

  onSearchChange(): void {
    this.queryParams$.next();
  }

  onSortChange(): void {
    this.queryParams$.next();
  }

  onSortDirectionChange(): void {
    this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    this.queryParams$.next();
  }

  onCurrencyChange(event: Event): void {
    const newCurrency = (event.target as HTMLSelectElement).value;

    if (newCurrency) {
      this.userSettingsService.patch({
        currency: newCurrency
      }).subscribe({
        next: () => {
          this.mainCurrency = newCurrency;
          this.popupMessageService.success(`Shown currency updated to <b>${this.mainCurrency}</b>`);
        }
      });
    }
  }

  toggleEditDialog(capital: CapitalResponse) {
    const ref = this.dialogService.open(CapitalDetailsComponent, {
      data: { capital }
    });

    ref
      .afterClosed$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (request: UpdateCapitalRequest | null) => {
          if (request) {
            this.capitalService
              .update(request.id, request)
              .pipe(takeUntilDestroyed(this.destroyRef))
              .subscribe({
                next: () => this.capitalUpdated(request),
                error: () => this.popupMessageService.error('Failed to update capital')
              });
          }
        }
      });
  }

  capitalUpdated(request: UpdateCapitalRequest): void {
    const capital = this.capitals.find(c => c.id === request.id);

    if (capital) {
      capital.name = request.name ?? capital.name;
      capital.balance = request.balance ?? capital.balance;
      capital.currency = request.currency ? request.currency : capital.currency;
      capital.includeInTotal = request.includeInTotal ?? capital.includeInTotal;
      capital.onlyForSavings = request.onlyForSavings ?? capital.onlyForSavings;

      this.popupMessageService.success(`${capital.name} was successfully updated.`);
    } else {
      this.popupMessageService.error('Error while updating occurred.');
    }
  }

  openToCreateCapitalDialog(): void {
    const dialogRef = this.dialogService.open(AddCapitalDialogComponent);

    dialogRef
      .afterClosed$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (request: AddCapitalRequest | null) => {
          if (request) {
            this.capitalService.create(request)
              .pipe(takeUntilDestroyed(this.destroyRef))
              .subscribe({
                next: (id) => {
                  this.addCapitalToTheList(id, request);
                },
                error: () => this.popupMessageService.error('Failed to create capital')
              });
          }
        }
      }
    );
  }

  addCapitalToTheList(id: number, request: AddCapitalRequest): void {
    const response: CapitalResponse = {
      id: id,
      name: request.name,
      balance: Number(request.balance),
      currency: request.currency,
      includeInTotal: request.includeInTotal,
      onlyForSavings: request.onlyForSavings,
      totalIncome: 0,
      totalExpense: 0,
      totalTransferIn: 0,
      totalTransferOut: 0,
      createdAt: new Date(),
      createdBy: this.user?.fullname ?? 'Anonymous'
    };

    this.capitals.push(response);
    this.popupMessageService.success(`${request.name} successfully added.`);
  }

  get totalCapitalBalance(): number {
    return this.capitals.reduce(
      (sum, capital) => capital.includeInTotal
        ? sum + convertCurrency(Number(capital.balance) || 0, capital.currency, this.mainCurrency, this.exchanges)
        : sum,
      0
    );
  }

  toggleIncludeInTotal(id: number): void {
    const capital = this.capitals.find(c => c.id === id);

    if (capital) {
      this.capitalService
        .patchIncludeTotal(id, !capital.includeInTotal)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            capital.includeInTotal = !capital.includeInTotal;
            this.popupMessageService.success(`${capital.name} ${capital.includeInTotal ? 'included into' : 'excluded from'} total balance`);
          },
          error: () => this.popupMessageService.error('Failed to update capital')
        });
    }
  }

  toggleSavingsOnly(id: number): void {
    const capital = this.capitals.find(c => c.id === id);

    if (capital) {
      this.capitalService
        .patchSavingsOnly(id, !capital.onlyForSavings)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            capital.onlyForSavings = !capital.onlyForSavings;
            this.popupMessageService.success(`${capital.name} set to ${capital.onlyForSavings ? 'only for savings' : 'regular'} capital`);
          },
          error: () => this.popupMessageService.error('Failed to update capital')
        });
    }
  }

  deleteCapital(id: number): void {
    const ref = this.dialogService.open(ConfirmDialogComponent, {
      data: {
        title: 'Deletion of capital',
        message: `
          Are you sure you want to perform this action?<br><br>
          <b>Note!</b> It may contain references and cannot be deleted.`,
        icon: 'danger'
      },
    });

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (confirmed: boolean) => {
        if (confirmed) {
          this.capitalService
            .delete(id)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: () => {
                this.capitals = this.capitals.filter(x => x.id !== id);
                this.popupMessageService.success("The capital was successful deleted.");
              },
              error: () => this.popupMessageService.error('Failed to delete capital')
            });
        }
      }
    });
  }

  drop(event: CdkDragDrop<any[]>) {
    moveItemInArray(this.capitals, event.previousIndex, event.currentIndex);

    const orders = this.capitals.map((c, index) => ({
      id: c.id,
      orderIndex: index
    }));

    this.capitalService
      .updateOrder({ capitals: orders })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.popupMessageService.success("Sort order is changed.");
        },
        error: () => this.popupMessageService.error('Failed to update sort order')
      });
  }
}
