import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { CURRENCY_DEFAULT } from '../../../../core/types/currency-type';
import { SharedModule } from '../../../../shared/shared.module';
import { DashboardSummary } from '../../models/dashboard-summary.model';
import { DashboardStore } from '../../services/dashboard-store.service';

@Component({
  selector: 'app-finance-dashboard',
  templateUrl: './finance-dashboard.component.html',
  styleUrl: './finance-dashboard.component.scss',
  standalone: true,
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FinanceDashboardComponent {
  protected readonly store = inject(DashboardStore);
  protected readonly defaultCurrency = CURRENCY_DEFAULT;

  summary = input.required<DashboardSummary>();

  protected readonly profit = computed(() => {
    const summary = this.summary();
    const factor = this.store.planFactor();

    return {
      amount: (summary.totalIncome - summary.totalExpense) * factor,
      inDefaultCurrency: (summary.totalIncomeInDefaultCurrency - summary.totalExpenseInDefaultCurrency) * factor
    };
  });
}
