import { Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { Exchange } from '../../core/models/exchange-model';
import { ConfirmDialogComponent } from '../../shared/components/dialogs/confirm-dialog/confirm-dialog.component';
import { DialogService } from '../../shared/services/dialog.service';
import { ExchangeService } from '../../shared/services/exchange.service';
import { PopupMessageService } from '../../shared/services/popup-message.service';
import { AddCapitalDialogComponent } from './components/capital-dialog/add-capital-dialog.component';
import { AddCapitalRequest } from './models/add-capital-request';
import { CapitalItem } from './models/capital-item';
import { CapitalResponse } from './models/capital-response';
import { CapitalService } from './services/capital.service';
import { CapitalDetailsComponent } from './components/capital-details/capital-details.component';
import { animate, style, transition, trigger } from '@angular/animations';
import { CurrencyType } from '../../core/types/currency-type';
import { getCurrencies } from '../../shared/components/currency/functions/get-currencies.component';
import { UpdateCapitalRequest } from './models/update-capital-request';
import { stringToCurrencyEnum } from '../../shared/components/currency/functions/string-to-currency-enum';

@Component({
  selector: 'app-capitals',
  templateUrl: './capitals.component.html',
  styleUrl: './capitals.component.scss',
  animations: [
    trigger('slideRemove', [
      transition(':leave', [
        animate('300ms ease', style({ transform: 'traslateX(100%)', opacity: 0 }))
      ]),
      transition(':enter', [
        style({ transform: 'translateX(-100%)', opacity: 0 }),
        animate('300ms ease', style({ transform: 'translateX(0)', opacity: 1 }))
      ])
    ])
  ]
})
export class CapitalsComponent implements OnInit, OnDestroy {
  capitals: CapitalResponse[] = [];
  sortedCapitals: CapitalResponse[] = [];
  capitalStatItems: CapitalItem[] = [
    { key: 'totalIncome', title: 'Incomes', icon: 'fa-dollar-sign', style: 'cp-incomes' },
    { key: 'totalExpense', title: 'Expenses', icon: 'fa-money-bill-wave', style: 'cp-expenses' },
    { key: 'totalTransferOut', title: 'Transfer Out', icon: 'fa-arrow-up', style: 'text-blue-400' },
    { key: 'totalTransferIn', title: 'Transfer In', icon: 'fa-arrow-down', style: 'text-pink-400' },
  ];
  exchanges: Exchange[] = [];

  searchTerm: string = '';
  selectedSortOption: keyof CapitalResponse = 'name';
  sortDirection: 'asc' | 'desc' = 'asc';

  mainCurrency: string = 'UAH';
  mainCurrencyVal: CurrencyType = stringToCurrencyEnum(this.mainCurrency) ?? CurrencyType.None;

  sortOptions: {label: string; key: keyof CapitalResponse}[] = [
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

  private unsubcribe$ = new Subject<void>;

  constructor(
    private readonly capitalService: CapitalService,
    private readonly popupMessageService: PopupMessageService,
    private readonly exchangeService: ExchangeService,
    private readonly dialogService: DialogService
  ) { }

  @HostListener('document:click', ['$event.target'])
  onClickOutside(targetElement: HTMLElement) {
    const clickedInside = targetElement.closest('.cp-actions-compact');

    if (!clickedInside) {
      this.onMenuItemClick();
    }
  }

  ngOnInit(): void {
    this.fetchCapitals();
    this.fetchExchanges();
  }

  ngOnDestroy(): void {
    this.unsubcribe$.next();
    this.unsubcribe$.complete();
  }

  fetchExchanges(): void {
    this.exchangeService
      .getAll()
      .pipe(takeUntil(this.unsubcribe$))
      .subscribe({
        next: (response) => this.exchanges = response
      });
  }

  fetchCapitals(): void {
    this.capitalService.getAll()
      .pipe(takeUntil(this.unsubcribe$))
      .subscribe({
        next: (response) => {
          this.capitals = response;
          this.applyFilters();
        }
    });
  }

  onSearchChange() {
    this.applyFilters();
  }

  onSortChange(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    this.selectedSortOption = selectElement.value as keyof CapitalResponse;
    this.applyFilters();
  }

  toggleSortDirection(): void {
    this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';

    this.applyFilters();
  }

  applyFilters(): void {
    const term = this.searchTerm.trim().toLowerCase();

    let searchedCapitals = this.capitals.filter(capital =>
      capital.name.toLowerCase().includes(term)
    );

    this.sortedCapitals = searchedCapitals.sort((a, b) => {
      const key = this.selectedSortOption;

      let valA = a[key];
      let valB = b[key];

      if (valA === undefined || valA === null) return 1;
      if (valB === undefined || valB === null) return -1;

      if (typeof valA === 'string' && typeof valB === 'string') {
        valA = valA.toLowerCase();
        valB = valB.toLowerCase();

        if (valA < valB) return this.sortDirection === 'asc' ? -1 : 1;
        if (valA > valB) return this.sortDirection === 'asc' ? 1 : -1;

        return 0;
      }

      if (typeof valA === 'number' && typeof valB === 'number') {
        return this.sortDirection === 'asc'
          ? valA - valB
          : valB - valA;
      }

      return 0;
    });
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

    this.dialogService.open({
      component: CapitalDetailsComponent,
      data: {
        capital: capital,
        currencyOptions: this.currencyOptions,
        exchanges: this.exchanges
      },
      onSubmit: (request: UpdateCapitalRequest | null) => {
        if (request) {
          this.capitalService
            .update(request.id, request)
            .pipe(takeUntil(this.unsubcribe$))
            .subscribe({
              next: () => this.capitalUpdated(request)
            });
        }
        this.dialogService.close();
      }
    })
  }

  capitalUpdated(request: UpdateCapitalRequest): void {
    const capital = this.sortedCapitals.find(c => c.id === request.id);

    if (capital) {
      capital.name = request.name ?? capital.name;
      capital.balance = request.balance ?? capital.balance;
      capital.currency = request.currency ? CurrencyType[request.currency] : capital.currency;
      capital.includeInTotal = request.includeInTotal ?? capital.includeInTotal;

      this.popupMessageService.success(`${capital.name} was successfully updated.`);
    } else {
      this.popupMessageService.error('Error while updating occured.');
    }
  }

  openToCreateCapitalDialog(): void {
    this.onMenuItemClick();

    this.dialogService.open({
      component: AddCapitalDialogComponent,
      data: {
        currencyOptions: this.currencyOptions
      },
      onSubmit: (request: AddCapitalRequest | null) => {
        if (request) {
          this.capitalService.create(request)
            .pipe(takeUntil(this.unsubcribe$))
            .subscribe({
              next: (id) => this.addCapitalToTheList(id, request)
            });
        }
        this.dialogService.close();
      }
    });
  }

  addCapitalToTheList(id: number, request: AddCapitalRequest): void {
    const response: CapitalResponse = {
      id: id,
      name: request.name,
      balance: Number(request.balance),
      currency: CurrencyType[request.currency],
      includeInTotal: request.includeInTotal,
      totalIncome: 0,
      totalExpense: 0,
      totalTransferIn: 0,
      totalTransferOut: 0
    };

    this.sortedCapitals.push(response);
    this.popupMessageService.success(`${request.name} successfully added.`);
  };

  totalCapitalAmount(): number {
    return this.sortedCapitals?.reduce((accumulator, capital) => {
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

  removeCapital(id: number): void {
    this.onMenuItemClick();

    this.dialogService.open({
      component: ConfirmDialogComponent,
      data: {
        title: 'removal capital',
        action: 'remove'
      },
      onSubmit: (confirmed: boolean) => {
        if (confirmed) {
          this.capitalService
            .delete(id)
            .pipe(takeUntil(this.unsubcribe$))
            .subscribe({
              next: () => {
                this.sortedCapitals = this.sortedCapitals.filter(x => x.id !== id);
                this.popupMessageService.success("The capital was successful removed.");
              }});
        }
        this.dialogService.close();
      }
    })
  }

  onMenuItemClick() {
    this.isCapitalActionsOpen = false;
    this.openMenuId = null;
  }
}
