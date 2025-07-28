import { Component, OnDestroy, OnInit } from '@angular/core';
import { Exchange } from '../../models/exchange-model';
import { ExchangeService } from '../../../shared/services/exchange.service';
import { Subject, takeUntil } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { OverlayRef } from '@angular/cdk/overlay';
import { ExchangeDialogComponent } from '../../../shared/components/exchange-dialog/exchange-dialog.component';

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
    { label: 'Capitals', icon: 'account_balance_wallet', link: '/capitals' },
    { label: 'Incomes', icon: 'attach_money', link: '/incomes' },
    { label: 'Expenses', icon: 'money_off', link: '/expenses' },
    { label: 'Transfers', icon: 'import_export', link: '' },
    { label: 'Goals', icon: 'star', link: '' }
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
