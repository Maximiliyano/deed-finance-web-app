import { Component, OnDestroy, OnInit } from '@angular/core';
import { Exchange } from '../../models/exchange-model';
import { ExchangeService } from '../../../shared/services/exchange.service';
import { Subject, takeUntil } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { OverlayRef } from '@angular/cdk/overlay';
import { ExchangeDialogComponent } from '../../../shared/components/dialogs/exchange-dialog/exchange-dialog.component';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent implements OnInit, OnDestroy {
  exchanges: Exchange[] | null;
  isSidebarExpanded: boolean;
  overlayRef: OverlayRef;
  navItems = [
    { label: 'Capitals', icon: 'fa-wallet', link: '/capitals' },
    { label: 'Goals', icon: 'fa-star', link: '/goals' },
    { label: 'Incomes', icon: 'fa-dollar-sign', link: '/incomes' },
    { label: 'Expenses', icon: 'fa-money-bill-wave', link: '/expenses' },
    { label: 'Transfers', icon: 'fa-exchange-alt', link: '/transfers' },
  ];

  private unsubscribe = new Subject<void>();

  constructor(
    private readonly dialog: MatDialog,
    private readonly exchangeService: ExchangeService) { }

  ngOnInit(): void {
    this.exchangeService
      .getAll()
      .pipe(
        takeUntil(this.unsubscribe))
      .subscribe(
        (exchanges) => {
          this.exchanges = exchanges.slice(0, 3);
        },
      (error) => console.error(error));
  }

  ngOnDestroy(): void {
    this.unsubscribe.complete();
    this.overlayRef.dispose();
  }

  openExchangeDialog(): void {
    this.dialog.open(ExchangeDialogComponent, {
      data: this.exchanges
    });
  }
}
