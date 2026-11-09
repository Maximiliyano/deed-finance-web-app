import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject } from '@angular/core';
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
import { DashboardStore } from './services/dashboard-store.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
  standalone: true,
  imports: [
    RouterLink,
    SharedModule,
    FinanceDashboardComponent,
    BudgetPlannerComponent,
    CapitalsPanelComponent,
    ExpensesPanelComponent,
    IncomesPanelComponent,
    GoalsPanelComponent,
    DebtsPanelComponent
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent implements OnInit {
  protected readonly store = inject(DashboardStore);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly navItems: NavItem[] = [
    { label: 'Budget Planner', icon: 'fa-compass-drafting', link: '/', fragment: 'budgetEstimations' },
    { label: 'Capitals', icon: 'fa-wallet', link: '/capitals' },
    { label: 'Expenses', icon: 'fa-money-bill-wave', link: '/expenses' },
    { label: 'Incomes', icon: 'fa-dollar-sign', link: '/incomes' },
    { label: 'Goals', icon: 'fa-star', link: '/', fragment: 'goals' },
    { label: 'Debts', icon: 'fa-hand-holding-dollar', link: '/', fragment: 'debts' }
  ];

  ngOnInit(): void {
    document.title = 'Deed - Home page';

    this.route.fragment.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(fragment => {
      if (fragment) {
        setTimeout(() => document.getElementById(fragment)?.scrollIntoView({ behavior: 'smooth', block: 'start' }), 300);
      }
    });
  }
}
