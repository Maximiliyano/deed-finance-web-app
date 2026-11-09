import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, TrackByFunction, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Exchange } from '../../models/exchange-model';
import { ExchangeService } from '../../../shared/services/exchange.service';

import { ExchangeDialogComponent } from '../../../shared/components/dialogs/exchange-dialog/exchange-dialog.component';
import { ProfileComponent } from '../../../modules/auth/components/profile/profile.component';
import { Router } from '@angular/router';
import { DialogService } from '../../../shared/components/dialogs/services/dialog.service';
import { AuthService } from '../../../modules/auth/services/auth-service';
import { User } from '../../../modules/auth/models/user';
import { ThemeService } from '../../services/theme.service';

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
export class HeaderComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly dialogService = inject(DialogService);
  private readonly exchangeService = inject(ExchangeService);
  readonly themeService = inject(ThemeService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  exchanges: Exchange[] = [];
  exchangeUpdatedAt: Date | null = null;
  isSidebarExpanded = false;
  openDropdown: string | null = null;
  expandedSidebarGroup: string | null = null;

  navItems: NavItem[] = [
    {
      label: 'Dashboard', icon: 'fa-gauge-high', children: [
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

  trackByLabel: TrackByFunction<NavItem>;
  trackBySale: TrackByFunction<Exchange>;

  ngOnInit(): void {
    this.authService.user$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(user => {
        this.user = user;
        this.cdr.markForCheck();
      });

    this.exchangeService
      .getLatest()
      .pipe(takeUntilDestroyed(this.destroyRef))
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
    this.dialogService.open(ProfileComponent);
  }
}
