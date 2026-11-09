import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { SharedModule } from '../../../../shared/shared.module';
import { CURRENCY_DEFAULT } from '../../../../core/types/currency-type';
import { DashboardSummary } from '../../models/dashboard-summary.model';

@Component({
  selector: 'app-finance-dashboard',
  templateUrl: './finance-dashboard.component.html',
  styleUrl: './finance-dashboard.component.scss',
  standalone: true,
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FinanceDashboardComponent {
  currency = input<string>('');
  planFilter = input<'day' | 'month' | 'year'>('month');
  summary = input<DashboardSummary | null>(null);

  private get planFactor(): number {
    switch (this.planFilter()) {
      case 'day':  return 1 / 30;
      case 'year': return 12;
      default:     return 1;
    }
  }

  get planFilterLabel(): string {
    const filter = this.planFilter();
    return filter === 'day' ? 'Daily' : filter === 'year' ? 'Yearly' : 'Monthly';
  }

  get showDualCurrency(): boolean {
    return this.currency() !== CURRENCY_DEFAULT;
  }

  get totalCapitalAmount(): number {
    return this.summary()?.totalCapital ?? 0;
  }

  get totalCapitalAmountInUah(): number {
    return this.summary()?.totalCapitalInDefaultCurrency ?? 0;
  }

  get totalIncomeAmount(): number {
    return this.summary()?.totalIncome ?? 0;
  }

  get totalIncomeAmountInUah(): number {
    return this.summary()?.totalIncomeInDefaultCurrency ?? 0;
  }

  get totalExpenseAmount(): number {
    return this.summary()?.totalExpense ?? 0;
  }

  get totalExpenseAmountInUah(): number {
    return this.summary()?.totalExpenseInDefaultCurrency ?? 0;
  }

  get totalProfit(): number {
    return this.totalIncomeAmount - this.totalExpenseAmount;
  }

  get isProfitable(): boolean {
    return this.totalProfit > 0;
  }

  get profitPerTime(): number {
    return this.totalProfit * this.planFactor;
  }

  get profitPerTimeInUah(): number {
    return (this.totalIncomeAmountInUah - this.totalExpenseAmountInUah) * this.planFactor;
  }
}
