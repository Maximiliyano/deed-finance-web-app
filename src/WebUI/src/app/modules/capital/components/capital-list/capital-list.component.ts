import {Component, HostListener, OnDestroy, OnInit} from '@angular/core';
import {Subject, takeUntil} from 'rxjs';
import {CapitalService} from '../../services/capital.service';
import {PopupMessageService} from '../../../../shared/services/popup-message.service';
import {ExchangeService} from "../../../../shared/services/exchange.service";
import {Exchange} from "../../../../core/models/exchange-model";
import { currencyToSymbol } from '../../../../shared/components/currency/functions/currencyToSymbol.component';
import { CapitalResponse } from '../../models/capital-response';
import { AddCapitalDialogComponent } from '../capital-dialog/add-capital-dialog.component';
import { DialogService } from '../../../../shared/services/dialog.service';
import { AddCapitalRequest } from '../../models/add-capital-request';
import { CapitalItem } from '../../models/capital-item';
import { ConfirmDialogComponent } from '../../../../shared/components/dialogs/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-capital-list',
  templateUrl: './capital-list.component.html',
  styleUrl: './capital-list.component.scss',
})
export class CapitalListComponent implements OnInit, OnDestroy {
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

  private unsubcribe$ = new Subject<void>;

  constructor(
    private readonly capitalService: CapitalService,
    private readonly popupMessageService: PopupMessageService,
    private readonly exchangeService: ExchangeService,
    private readonly dialogService: DialogService
  ) { }

  @HostListener('document:click', ['$event.target'])
  onClickOutside(targetElement: HTMLElement) {
    const clickedInside = targetElement.closest('.cp-actions');

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
    console.log(this.selectedSortOption);
    this.applyFilters();
  }

  toggleSortDirection(): void {
    this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';

    this.applyFilters();
  }

  applyFilters(): void {
    const term = this.searchTerm.trim().toLowerCase();

    let filtered = this.capitals.filter(capital =>
      capital.name.toLowerCase().includes(term)
    );

    filtered = filtered.sort((a, b) => {
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
    console.log(filtered);

    this.sortedCapitals = filtered;
  }

  symbol(value: string): string {
    return currencyToSymbol(value);
  }

  onCurrencyChange(newCurrency: string): void {
    this.popupMessageService.success(`The default currency updated to <b>${newCurrency}</b>`);
    this.mainCurrency = newCurrency;
  }

  openToCreateCapitalDialog(): void {
    this.onMenuItemClick();

    this.dialogService.open({
      component: AddCapitalDialogComponent,
      onSubmit: (request: AddCapitalRequest) => {
        if (request) {
          this.capitalService.create(request)
            .pipe(takeUntil(this.unsubcribe$))
            .subscribe({
              next: (id) => this.addCapitalToTheList(id, request)
            });
        }
      }
    });
  }

  addCapitalToTheList(id: number, request: AddCapitalRequest): void {
    const response: CapitalResponse = {
      id: id,
      name: request.name,
      balance: request.balance,
      currency: request.currency,
      includeInTotal: request.includeInTotal,
      totalIncome: 0,
      totalExpense: 0,
      totalTransferIn: 0,
      totalTransferOut: 0
    };

    this.capitals.push(response);
    this.popupMessageService.success("Capital successfully added.");
    this.dialogService.close();
  };

  totalCapitalAmount(): number {
    return this.capitals?.reduce((accumulator, capital) => {
      if (!capital.includeInTotal) return accumulator;

      if (capital.currency === this.mainCurrency) {
        return accumulator + capital.balance;
      }

      const exchange = this.exchanges?.find(e => e.nationalCurrency === this.mainCurrency && e.targetCurrency === capital.currency);
      return exchange
        ? accumulator + capital.balance * exchange.sale
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
                this.capitals = this.capitals.filter(x => x.id !== id);
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
