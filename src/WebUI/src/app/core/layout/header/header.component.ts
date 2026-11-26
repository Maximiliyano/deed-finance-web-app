import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, inject, DestroyRef } from '@angular/core';
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
import { ScrollLockService } from '../../services/scroll-lock.service';

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

  private readonly scrollLock = inject(ScrollLockService);
  private sidebarOpen = false;

  exchanges: Exchange[] = [];

  get isSidebarExpanded(): boolean {
    return this.sidebarOpen;
  }

  set isSidebarExpanded(open: boolean) {
    if (open === this.sidebarOpen) return;

    this.sidebarOpen = open;
    if (open) {
      this.scrollLock.lock();
    } else {
      this.scrollLock.unlock();
    }
  }

  openDropdown: string | null = null;
  expandedSidebarGroup: string | null = null;

  navItems: NavItem[] = [
    {
      label: 'Dashboard',
      icon: 'fa-gauge-high',
      children: [
        { label: 'Budget Planner', icon: 'fa-compass-drafting', link: '/', fragment: 'budgetEstimations' },
        { label: 'Capitals', icon: 'fa-wallet', link: '/', fragment: 'capitals' },
        { label: 'Expenses', icon: 'fa-money-bill-wave', link: '/', fragment: 'expenses' },
        { label: 'Incomes', icon: 'fa-arrow-trend-up', link: '/', fragment: 'incomes' },
        { label: 'Utility Bills', icon: 'fa-bolt', link: '/', fragment: 'utilityBills' },
        { label: 'Goals', icon: 'fa-star', link: '/', fragment: 'goals' },
        { label: 'Debts', icon: 'fa-hand-holding-dollar', link: '/', fragment: 'debts' }
      ]
    }
  ];
  user: User | null;

  ngOnInit(): void {
    this.destroyRef.onDestroy(() => {
      this.isSidebarExpanded = false;
    });

    this.authService.user$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(user => {
      this.user = user;
      this.cdr.markForCheck();
    });

    this.exchangeService
      .getLatest()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: exchanges => {
          this.exchanges = exchanges;
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
    this.isSidebarExpanded = false;
    this.dialogService.open(ProfileComponent);
  }
}
