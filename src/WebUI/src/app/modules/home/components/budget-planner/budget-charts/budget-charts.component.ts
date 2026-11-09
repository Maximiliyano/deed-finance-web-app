import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { SharedModule } from '../../../../../shared/shared.module';
import { CategoryAmount } from '../../../models/dashboard-summary.model';
import { DashboardStore } from '../../../services/dashboard-store.service';
import { BUDGET_BAR_COLORS } from '../budget-planner.constants';

type ChartTab = 'general' | 'expenses' | 'incomes';

interface ChartItem {
  name: string;
  amount: number;
  pct: number;
  color: string;
}

@Component({
  selector: 'app-budget-charts',
  templateUrl: './budget-charts.component.html',
  styleUrl: './budget-charts.component.scss',
  standalone: true,
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BudgetChartsComponent {
  protected readonly store = inject(DashboardStore);

  protected readonly chartTab = signal<ChartTab>('general');

  protected readonly scaledFilteredNet = computed(() => this.store.filteredNetProfit() * this.store.planFactor());
  protected readonly scaledFilteredIncomes = computed(() => this.store.filteredIncome() * this.store.planFactor());
  protected readonly scaledFilteredLoss = computed(() => this.store.filteredLoss() * this.store.planFactor());

  protected readonly donutLossPct = computed(() => {
    const income = this.store.filteredIncome();
    const loss = this.store.filteredLoss();

    if (income <= 0) return loss > 0 ? 100 : 0;

    return Math.min(100, Math.max(0, (loss / income) * 100));
  });

  protected readonly donutNetPct = computed(() => Math.max(0, 100 - this.donutLossPct()));

  protected readonly incomeItems = computed(() => this.toChartItems(this.store.chartSummary()?.incomesByCategory));
  protected readonly expenseItems = computed(() => this.toChartItems(this.store.chartSummary()?.expensesByCategory));

  protected readonly totalExpense = computed(() => this.store.chartSummary()?.totalExpense ?? 0);

  private toChartItems(items: CategoryAmount[] = []): ChartItem[] {
    return items.map((item, index) => ({
      name: item.name,
      amount: item.amount,
      pct: item.percentage,
      color: BUDGET_BAR_COLORS[index % BUDGET_BAR_COLORS.length]
    }));
  }
}
