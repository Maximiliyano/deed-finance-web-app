import { Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { debounceTime, Subject, takeUntil } from 'rxjs';
import { Exchange } from '../../core/models/exchange-model';
import { DialogService } from '../../shared/components/dialogs/services/dialog.service';
import { ExchangeService } from '../../shared/services/exchange.service';
import { PopupMessageService } from '../../shared/services/popup-message.service';
import { AddCapitalRequest } from './models/add-capital-request';
import { CapitalItem } from './models/capital-item';
import { CapitalResponse } from './models/capital-response';
import { CapitalService } from './services/capital.service';
import { animate, style, transition, trigger } from '@angular/animations';
import { CurrencyType } from '../../core/types/currency-type';
import { getCurrencies } from '../../shared/components/currency/functions/get-currencies.component';
import { UpdateCapitalRequest } from './models/update-capital-request';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { AddCapitalDialogComponent } from './components/capital-dialog/add-capital-dialog.component';
import { CapitalDetailsComponent } from './components/capital-details/capital-details.component';
import { ConfirmDialogComponent } from '../../shared/components/dialogs/confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-capitals',
    templateUrl: './capitals.component.html',
    styleUrl: './capitals.component.scss',
    animations: [
        trigger('slideRemove', [
            transition(':leave', [
                animate('350ms ease', style({ transform: 'traslateY(100%)', opacity: 0 }))
            ]),
            transition(':enter', [
                style({ transform: 'translateY(-100%)', opacity: 0 }),
                animate('350ms ease', style({ transform: 'translateY(0)', opacity: 1 }))
            ])
        ])
    ],
    standalone: false
})
export class CapitalsComponent implements OnInit, OnDestroy {
  capitals: CapitalResponse[] = [];
  capitalStatItems: CapitalItem[] = [
    { key: 'totalIncome', title: 'Incomes', icon: 'fa-dollar-sign', style: 'cp-incomes' },
    { key: 'totalExpense', title: 'Expenses', icon: 'fa-money-bill-wave', style: 'cp-expenses', url: '/expenses' },
    { key: 'totalTransferOut', title: 'Transfer Out', icon: 'fa-arrow-up', style: 'text-blue-400' },
    { key: 'totalTransferIn', title: 'Transfer In', icon: 'fa-arrow-down', style: 'text-pink-400' },
  ];
  exchanges: Exchange[] = [];

  searchTerm = '';
  selectedSortOption: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';
  filterBy: 'onlyForSavings' | null = null;

  mainCurrency: string;
  mainCurrencyVal: CurrencyType;

  sortOptions: {label: string; key: string}[] = [
    { label: 'user order', key: '' },
    { label: 'name', key: 'name' },
    { label: 'balance', key: 'balance' },
    { label: 'expenses', key: 'totalExpense' },
    { label: 'incomes', key: 'totalIncome' },
    { label: 'transfers in', key: 'totalTransferIn' },
    { label: 'transfers out', key: 'totalTransferOut' },
  ];

  createDialogOpened: boolean = false;

  isCapitalActionsOpen: boolean = false;
  openMenuId: number | null = null;

  currencyOptions = getCurrencies({ excludeNone: true });

  private queryParams$ = new Subject<void>();
  private unsubcribe$ = new Subject<void>();

  constructor(
    private readonly capitalService: CapitalService,
    private readonly popupMessageService: PopupMessageService,
    private readonly exchangeService: ExchangeService,
    private readonly dialogService: DialogService
  ) {
  }

  capitalBackgroundClass(includeInTotal: boolean, onlyForSavings: boolean) {
    if (onlyForSavings) {
      return 'cp bg-gradient-to-r from-white to-yellow-600';
    }
    if (!includeInTotal) {
      return 'cp bg-gradient-to-r from-white to-blue-950';
    }
    else {
      return 'cp bg-gradient-to-r from-white to-gray-100';
    }
  }

  @HostListener('document:click', ['$event.target'])
  onClickOutside(targetElement: EventTarget | null): void {
    if (!(targetElement instanceof HTMLElement)) {
      return;
    }

    const clickedInside = targetElement.closest('.cp-actions-compact');

    if (!clickedInside) {
      this.onMenuItemClick();
    }
  }

  ngOnInit(): void {
    document.title = "Deed - Capitals";

    const mainCurrency = this.capitalService.getMainCurrency();
    this.mainCurrency = mainCurrency.str;
    this.mainCurrencyVal = mainCurrency.val;

    this.queryParams$
      .pipe(
        debounceTime(300),
        takeUntil(this.unsubcribe$)
      )
      .subscribe({
        next: () => this.fetchCapitals()
      })

    this.fetchCapitals();
    this.fetchExchanges();
  }

  ngOnDestroy(): void {
    this.unsubcribe$.next();
    this.unsubcribe$.complete();
    this.queryParams$.complete();
  }

  fetchExchanges(): void {
    this.exchangeService
      .getLatest()
      .pipe(takeUntil(this.unsubcribe$))
      .subscribe({
        next: (response) => this.exchanges = response
      });
  }

  fetchCapitals(): void {
    this.capitalService.getAll({
        searchTerm: this.searchTerm,
        sortBy: this.selectedSortOption,
        sortDirection: this.sortDirection,
        filterBy: this.filterBy
      })
      .pipe(takeUntil(this.unsubcribe$))
      .subscribe({
        next: (response) => {
          this.capitals = response;
        }
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
      this.mainCurrency = CurrencyType[Number(newCurrency)];
      this.popupMessageService.success(`The default currency updated to <b>${this.mainCurrency}</b>`);
    }
  }

  toggleEditDialog(capital: CapitalResponse) {
    this.onMenuItemClick();

    const ref = this.dialogService.open(CapitalDetailsComponent, {
      data: {
        capital: capital,
        currencyOptions: this.currencyOptions,
        exchanges: this.exchanges
      }
    });

    ref
      .afterClosed$
      .pipe(takeUntil(this.unsubcribe$))
      .subscribe({
        next: (request: UpdateCapitalRequest | null) => {
          if (request) {
            this.capitalService
              .update(request.id, request)
              .pipe(takeUntil(this.unsubcribe$))
              .subscribe({
                next: () => this.capitalUpdated(request)
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
      capital.currency = request.currency ? CurrencyType[request.currency] : capital.currency;
      capital.includeInTotal = request.includeInTotal ?? capital.includeInTotal;
      capital.onlyForSavings = request.onlyForSavings ?? capital.onlyForSavings;

      this.popupMessageService.success(`${capital.name} was successfully updated.`);
    } else {
      this.popupMessageService.error('Error while updating occured.');
    }
  }

  openToCreateCapitalDialog(): void {
    this.onMenuItemClick();

    const dialogRef = this.dialogService.open(AddCapitalDialogComponent, {
      data: this.currencyOptions
    });

    dialogRef
      .afterClosed$
      .subscribe({
        next: (request: AddCapitalRequest | null) => {
          if (request) {
            this.capitalService.create(request)
              .pipe(takeUntil(this.unsubcribe$))
              .subscribe({
                next: (id) => {
                  this.addCapitalToTheList(id, request);
                }
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
      currency: CurrencyType[request.currency],
      includeInTotal: request.includeInTotal,
      onlyForSavings: request.onlyForSavings,
      totalIncome: 0,
      totalExpense: 0,
      totalTransferIn: 0,
      totalTransferOut: 0,
      createdAt: new Date(),
      createdBy: 0
    };

    this.capitals.push(response);
    this.popupMessageService.success(`${request.name} successfully added.`);
  }

  get totalCapitalBalance(): number {
    return this.capitals?.reduce((accumulator, capital) => {
      if (!capital.includeInTotal) return accumulator;

      const balance = Number(capital.balance) || 0;

      if (capital.currency === this.mainCurrency) {
        return accumulator + balance;
      }

      const exchange = this.exchanges?.find(
        e => e.nationalCurrency === this.mainCurrency && e.targetCurrency === capital.currency
      );

      return exchange
        ? accumulator + balance * exchange.sale
        : accumulator;
    }, 0) ?? 0;
  }

  toggleMenu(id: number): void {
    if (this.openMenuId === id) {
      this.openMenuId = null;
      this.isCapitalActionsOpen = false;
    } else {
      this.openMenuId = id;
      this.isCapitalActionsOpen = true;
    }
  }

  toggleIncludeInTotal(id: number): void {
    const capital = this.capitals.find(c => c.id === id);

    if (capital) {
      this.capitalService
        .patchIncludeTotal(id, !capital.includeInTotal)
        .pipe(takeUntil(this.unsubcribe$))
        .subscribe({
          next: () => {
            capital.includeInTotal = !capital.includeInTotal;
            this.popupMessageService.success(`${capital.name} ${capital.includeInTotal ? 'included into' : 'excluded from'} total balance`);
          }
        });
    }
  }

  toggleSavingsOnly(id: number): void {
    const capital = this.capitals.find(c => c.id === id);

    if (capital) {
      this.capitalService
        .patchSavingsOnly(id, !capital.onlyForSavings)
        .pipe(takeUntil(this.unsubcribe$))
        .subscribe({
          next: () => {
            capital.onlyForSavings = !capital.onlyForSavings;
            this.popupMessageService.success(`${capital.name} set to ${capital.onlyForSavings ? 'only for savings' : 'regular'} capital`);
          }
        });
    }
  }

  deleteCapital(id: number): void {
    this.onMenuItemClick();

    const ref = this.dialogService.open(ConfirmDialogComponent, {
      data: {
        title: 'Deletion of capital',
        message: `
          Are you sure you want to perform this action?<br><br>
          <b>Note!</b> It may contain references and cannot be deleted.`,
        icon: 'danger'
      },
    });

    ref.afterClosed$.subscribe({
      next: (confirmed: boolean) => {
        if (confirmed) {
          this.capitalService
            .delete(id)
            .pipe(takeUntil(this.unsubcribe$))
            .subscribe({
              next: () => {
                this.capitals = this.capitals.filter(x => x.id !== id);
                this.popupMessageService.success("The capital was successful deleted.");
              }});
        }
      }
    });
  }

  onMenuItemClick(): void {
    this.isCapitalActionsOpen = false;
    this.openMenuId = null;
  }

  drop(event: CdkDragDrop<any[]>) {
    moveItemInArray(this.capitals, event.previousIndex, event.currentIndex);

    const orders = this.capitals.map((c, index) => ({
      id: c.id,
      orderIndex: index
    }));

    this.capitalService
      .updateOrder({ capitals: orders })
      .pipe(takeUntil(this.unsubcribe$))
      .subscribe({
        next: () => {
          this.popupMessageService.success("Sort order is changed.");
        }
      });
  }
}
