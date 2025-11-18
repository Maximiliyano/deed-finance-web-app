import { Component, OnDestroy, OnInit, TrackByFunction } from '@angular/core';
import { Exchange } from '../../models/exchange-model';
import { ExchangeService } from '../../../shared/services/exchange.service';
import { Subject, takeUntil } from 'rxjs';
import { ExchangeDialogComponent } from '../../../shared/components/dialogs/exchange-dialog/exchange-dialog.component';
import { DialogService } from '../../../shared/components/dialogs/services/dialog.service';
import { AuthService, User } from '../../../modules/auth/services/auth-service';
import { Router } from '@angular/router';

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss',
    standalone: false
})
export class HeaderComponent implements OnInit, OnDestroy {
  exchanges: Exchange[] = [];
  exchangeUpdatedAt: Date | null = null;
  isSidebarExpanded: boolean;
  navItems = [
    { label: 'Capitals', icon: 'fa-wallet', link: '/capitals' },
    { label: 'Expenses', icon: 'fa-money-bill-wave', link: '/expenses' },
    { label: 'Incomes', icon: 'fa-dollar-sign', link: '/incomes' },
    { label: 'Goals', icon: 'fa-star', link: '/goals' },
    { label: 'Transfers', icon: 'fa-exchange-alt', link: '/transfers' },
  ];
  user: User | null = null;

  private unsubscribe = new Subject<void>();
  
  trackByLabel: TrackByFunction<{ label: string; icon: string; link: string; }>;
  trackBySale: TrackByFunction<Exchange>;

  constructor(
    private readonly router: Router,
    private readonly authService: AuthService,
    private readonly dialogService: DialogService,
    private readonly exchangeService: ExchangeService) { }

  ngOnInit(): void {
    this.authService.me()
      .pipe(
        takeUntil(this.unsubscribe))
      .subscribe({
        next: (user) => this.user = user, 
        error: () => {}
      });

    this.exchangeService
      .getLatest()
      .pipe(takeUntil(this.unsubscribe))
      .subscribe({
        next: (exchanges) => {
          this.exchanges = exchanges;
          if (exchanges.length > 0) {
            this.exchangeUpdatedAt = exchanges[0].updatedAt;
          }
        }
      });
  }

  ngOnDestroy(): void {
    this.unsubscribe.next();
    this.unsubscribe.complete();
  }

  openExchangeDialog(): void {
    this.dialogService.open(ExchangeDialogComponent, {
      data: this.exchanges
    });
  }

  login(): void {
    this.authService.login();
  }

  logout(): void {
    this.authService.logout();
  }

  profile(): void {
    this.router.navigate(['./profile']);
  }
}
