import {Component, OnDestroy, OnInit} from '@angular/core';
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

@Component({
  selector: 'app-capital-list',
  templateUrl: './capital-list.component.html',
  styleUrl: './capital-list.component.scss',
})
export class CapitalListComponent implements OnInit, OnDestroy {
  capitals: CapitalResponse[];
  exchanges: Exchange[];
  mainCurrency: string = 'UAH';
  propsItems: { key: keyof CapitalResponse; title: string; icon: string, style: string }[] = [
    { key: 'totalIncome', title: 'Incomes', icon: 'fa-dollar-sign', style: 'cp-incomes' },
    { key: 'totalExpense', title: 'Expenses', icon: 'fa-money-bill-wave', style: 'cp-expenses' },
    { key: 'totalTransferOut', title: 'Transfer Out', icon: 'fa-arrow-up', style: '' },
    { key: 'totalTransferIn', title: 'Transfer In', icon: 'fa-arrow-down', style: '' },
  ];

  searchTerm: string = '';

  selectedSortOption: keyof CapitalResponse = 'name';
  sortDirection: 'asc' | 'desc' = 'asc';
  sortOptions: string[] = ['name', 'balance', 'expenses', 'incomes', 'transfers in', 'transfers out'];

  createDialogOpened: boolean = false;

  private unsubcribe$ = new Subject<void>;

  constructor(
    private readonly capitalService: CapitalService,
    private readonly popupMessageService: PopupMessageService,
    private readonly exchangeService: ExchangeService,
    private readonly dialogService: DialogService
  ) { }

    ngOnInit(): void {
      // TODO execute user capitals
      this.executeCapitals();

      this.executeExchanges();

      // TODO execute currency by default from user settings for 'mainCurrency'
    }

    ngOnDestroy(): void {
      this.unsubcribe$.next();
      this.unsubcribe$.complete();
    }

    onSearchChange() {
      const term = this.searchTerm.trim().toLowerCase();

      this.capitals = this.capitals.filter(capital =>
        capital.name.toLowerCase().includes(term)
      );

      this.sortCapitals(this.selectedSortOption);
    }

    onSortChange(event: Event): void {
      const selectElement = event.target as HTMLSelectElement;
      const key = selectElement.value as keyof CapitalResponse;

      this.sortCapitals(key);
    }

  toggleSortDirection(): void {
    this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';

    this.sortCapitals(this.selectedSortOption);
  }

  executeExchanges(): void {
    this.exchangeService
      .getAll()
      .pipe(takeUntil(this.unsubcribe$))
      .subscribe({
        next: (response) => this.exchanges = response
      });
  }

  executeCapitals(): void {
    this.capitalService.getAll()
      .pipe(takeUntil(this.unsubcribe$))
      .subscribe({
        next: (response) => {
          this.capitals = response;
        }
      });
  }

  symbol(value: string): string {
    return currencyToSymbol(value);
  }

  onCurrencyChange(newCurrency: string): void {
    // TODO update default currency for user here
    this.popupMessageService.success(`The default currency updated to <b>${newCurrency}</b>`);
    this.mainCurrency = newCurrency;
  }

  openToCreateCapitalDialog(): void {
    this.dialogService.open(AddCapitalDialogComponent, {
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
      includeInTotal: false,
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

  toggleVisible(capital: CapitalResponse): void {
    capital.includeInTotal = !capital.includeInTotal;
  }

  removeCapital(id: number): void {
    this.capitalService
      .delete(id)
      .pipe(takeUntil(this.unsubcribe$))
      .subscribe({
        next: () => {
          this.capitals = this.capitals.filter(x => x.id !== id);
          this.popupMessageService.success("The capital was successful removed.");
        }});
  }

  sortCapitals(key: keyof CapitalResponse) {
    if (!this.capitals) return;

    const dir = this.sortDirection === 'asc' ? 1 : -1;
    this.capitals = [...this.capitals].sort((a, b) => {
      const aVal = a[key];
      const bVal = b[key];

      if (typeof aVal === 'string') {
        return (aVal as string).localeCompare(bVal as string) * dir;
      }

      return ((aVal as number) - (bVal as number)) * dir;
    });
  }

}
