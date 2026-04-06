import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, TrackByFunction } from '@angular/core';
import { Exchange } from '../../models/exchange-model';
import { ExchangeService } from '../../../shared/services/exchange.service';
import { Subject, takeUntil } from 'rxjs';
import { ExchangeDialogComponent } from '../../../shared/components/dialogs/exchange-dialog/exchange-dialog.component';
import { Router } from '@angular/router';
import { DialogService } from '../../../shared/components/dialogs/services/dialog.service';
import { AuthService } from '../../../modules/auth/services/auth-service';
import { User } from '../../../modules/auth/models/user';
import { ViewportScroller } from '@angular/common';

export interface NavItem {
  label: string;
  icon: string;
  link?: string;
  fragment?: string;
  children?: NavItem[];
}

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss',
    standalone: false,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeaderComponent implements OnInit, OnDestroy {
  exchanges: Exchange[] = [];
  exchangeUpdatedAt: Date | null = null;
  isSidebarExpanded = false;
  openDropdown: string | null = null;
  expandedSidebarGroup: string | null = null;

  navItems: NavItem[] = [
    {
      label: 'Dashboard', icon: 'fa-gauge-high', children: [
        { label: 'Overview', icon: 'fa-house', link: '/' },
        { label: 'Budget Planner', icon: 'fa-compass-drafting', link: '/', fragment: 'estimations' },
        { label: 'Goals', icon: 'fa-star', link: '/', fragment: 'goals' },
        { label: 'Debts', icon: 'fa-hand-holding-dollar', link: '/', fragment: 'debts' },
      ]
    },
    {
      label: 'Finance', icon: 'fa-coins', children: [
        { label: 'Capitals', icon: 'fa-wallet', link: '/capitals' },
        { label: 'Expenses', icon: 'fa-money-bill-wave', link: '/expenses' },
        { label: 'Incomes', icon: 'fa-dollar-sign', link: '/incomes' }
      ]
    },
  ];
  user: User | null;

  private unsubscribe = new Subject<void>();

  trackByLabel: TrackByFunction<NavItem>;
  trackBySale: TrackByFunction<Exchange>;

  constructor(
    private readonly router: Router,
    private readonly authService: AuthService,
    private readonly dialogService: DialogService,
    private readonly exchangeService: ExchangeService,
    private readonly viewportScroller: ViewportScroller,
    private readonly cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.authService.user$
      .pipe(takeUntil(this.unsubscribe))
      .subscribe(user => {
        this.user = user;
        this.cdr.markForCheck();
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
          this.cdr.markForCheck();
        }
      });
  }

  ngOnDestroy(): void {
    this.unsubscribe.next();
    this.unsubscribe.complete();
  }

  navigateTo(item: NavItem): void {
    this.openDropdown = null;
    this.isSidebarExpanded = false;

    if (item.fragment) {
      this.router.navigate([item.link ?? '/'], { fragment: item.fragment }).then(() => {
        setTimeout(() => {
          const el = document.getElementById(item.fragment!);
          if (el) {
            el.scrollIntoView({ behavior: 'smooth', block: 'start' });
          }
        }, 100);
      });
    } else if (item.link) {
      this.router.navigate([item.link]);
    }
  }

  toggleDropdown(label: string, event: MouseEvent): void {
    event.stopPropagation();
    this.openDropdown = this.openDropdown === label ? null : label;
  }

  closeDropdowns(): void {
    this.openDropdown = null;
  }

  toggleSidebarGroup(label: string): void {
    this.expandedSidebarGroup = this.expandedSidebarGroup === label ? null : label;
  }

  openExchangeDialog(): void {
    this.dialogService.open(ExchangeDialogComponent, {
      data: this.exchanges
    });
  }

  login(): void {
    this.authService.login();
  }

  profile(): void {
    this.router.navigate(['./profile']);
  }
}
