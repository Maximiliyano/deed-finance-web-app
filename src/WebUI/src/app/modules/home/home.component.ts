import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { NavItem } from '../../core/layout/header/header.component';
import { SharedModule } from '../../shared/shared.module';
import { BudgetPlannerComponent } from './components/budget-planner/budget-planner.component';
import { CapitalsPanelComponent } from './components/capitals-panel/capitals-panel.component';
import { DebtsPanelComponent } from './components/debts-panel/debts-panel.component';
import { ExpensesPanelComponent } from './components/expenses-panel/expenses-panel.component';
import { FinanceDashboardComponent } from './components/finance-dashboard/finance-dashboard.component';
import { GoalsPanelComponent } from './components/goals-panel/goals-panel.component';
import { IncomesPanelComponent } from './components/incomes-panel/incomes-panel.component';
import { MobileDockComponent } from './components/mobile-dock/mobile-dock.component';
import { UtilityBillsPanelComponent } from './components/utility-bills-panel/utility-bills-panel.component';
import { DashboardStore } from './services/dashboard-store.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
  standalone: true,
  imports: [
    RouterLink,
    SharedModule,
    MobileDockComponent,
    FinanceDashboardComponent,
    BudgetPlannerComponent,
    CapitalsPanelComponent,
    ExpensesPanelComponent,
    IncomesPanelComponent,
    UtilityBillsPanelComponent,
    GoalsPanelComponent,
    DebtsPanelComponent
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent implements OnInit {
  protected readonly store = inject(DashboardStore);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly today = new Date().toLocaleDateString('en-GB', {
    weekday: 'long',
    day: 'numeric',
    month: 'long'
  });

  protected readonly greeting = computed(() => {
    const hour = new Date().getHours();
    const part =
      hour >= 5 && hour < 12
        ? 'Good morning'
        : hour >= 12 && hour < 17
          ? 'Good afternoon'
          : hour >= 17 && hour < 22
            ? 'Good evening'
            : 'Hello';
    const first = this.store.user()?.fullname?.trim().split(/\s+/)[0];

    return first ? `${part}, ${first}` : part;
  });

  protected readonly navItems: NavItem[] = [
    { label: 'Budget planner', icon: 'fa-compass-drafting', link: '/', fragment: 'budgetEstimations' },
    { label: 'Capitals', icon: 'fa-wallet', link: '/', fragment: 'capitals' },
    { label: 'Expenses', icon: 'fa-money-bill-wave', link: '/', fragment: 'expenses' },
    { label: 'Income', icon: 'fa-arrow-trend-up', link: '/', fragment: 'incomes' },
    { label: 'Utility bills', icon: 'fa-bolt', link: '/', fragment: 'utilityBills' },
    { label: 'Goals', icon: 'fa-star', link: '/', fragment: 'goals' },
    { label: 'Debts', icon: 'fa-hand-holding-dollar', link: '/', fragment: 'debts' }
  ];

  ngOnInit(): void {
    document.title = 'Deed';

    this.route.fragment.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(fragment => {
      if (fragment) {
        setTimeout(
          () => document.getElementById(fragment)?.scrollIntoView({ behavior: 'smooth', block: 'start' }),
          300
        );
      }
    });
  }
}
