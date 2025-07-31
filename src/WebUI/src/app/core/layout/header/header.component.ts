import { Component, OnDestroy, OnInit } from '@angular/core';
import { Exchange } from '../../models/exchange-model';
import { ExchangeService } from '../../../shared/services/exchange.service';
import { Subject, takeUntil } from 'rxjs';
import { ExchangeDialogComponent } from '../../../shared/components/dialogs/exchange-dialog/exchange-dialog.component';
import { DialogService } from '../../../shared/services/dialog.service';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent implements OnInit, OnDestroy {
  exchanges: Exchange[] = [];
  isSidebarExpanded: boolean;
  navItems = [
    { label: 'Capitals', icon: 'fa-wallet', link: '/capitals' },
    { label: 'Goals', icon: 'fa-star', link: '/goals' },
    { label: 'Incomes', icon: 'fa-dollar-sign', link: '/incomes' },
    { label: 'Expenses', icon: 'fa-money-bill-wave', link: '/expenses' },
    { label: 'Transfers', icon: 'fa-exchange-alt', link: '/transfers' },
  ];

  private unsubscribe = new Subject<void>();

  constructor(
    private readonly dialogService: DialogService,
    private readonly exchangeService: ExchangeService) { }

  ngOnInit(): void {
    this.exchangeService
      .getAll()
      .pipe(
        takeUntil(this.unsubscribe))
      .subscribe({
        next: (exchanges) => this.exchanges = exchanges.slice(0, 3)
      });
  }

  ngOnDestroy(): void {
    this.unsubscribe.complete();
  }

  openExchangeDialog(): void {
    this.dialogService.open({
      component: ExchangeDialogComponent,
      data: {
        exchanges: this.exchanges
      }
    });
  }
}
