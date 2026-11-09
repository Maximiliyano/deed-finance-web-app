import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { SharedModule } from '../../../../../shared/shared.module';
import { DashboardStore } from '../../../services/dashboard-store.service';

@Component({
  selector: 'app-budget-period-bar',
  templateUrl: './budget-period-bar.component.html',
  styleUrl: './budget-period-bar.component.scss',
  standalone: true,
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BudgetPeriodBarComponent {
  protected readonly store = inject(DashboardStore);

  protected readonly selectedDay = computed(() => {
    const d = this.store.selectedDate();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  });

  protected readonly selectedMonth = computed(() => {
    const d = this.store.selectedDate();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
  });

  protected readonly selectedYear = computed(() => this.store.selectedDate().getFullYear());

  protected readonly selectedPeriodLabel = computed(() => {
    const options: Intl.DateTimeFormatOptions =
      this.store.planFilter() === 'day' ? { year: 'numeric', month: 'short', day: 'numeric' } :
      this.store.planFilter() === 'year' ? { year: 'numeric' } :
      { year: 'numeric', month: 'long' };

    return this.store.selectedDate().toLocaleDateString(undefined, options);
  });

  protected readonly scaledLoss = computed(() => this.store.loss() * this.store.planFactor());
  protected readonly scaledLossInDefault = computed(() => this.store.lossInDefaultCurrency() * this.store.planFactor());
  protected readonly scaledProfit = computed(() => this.store.profit() * this.store.planFactor());
  protected readonly scaledProfitInDefault = computed(() => this.store.profitInDefaultCurrency() * this.store.planFactor());
  protected readonly isProfitable = computed(() => this.store.profit() >= 0);

  protected readonly lossPercent = computed(() => {
    const capital = this.store.totalCapital();
    return capital > 0 ? Math.min(100, Math.round((this.store.loss() / capital) * 100)) : 0;
  });

  protected onDayChange(value: string): void {
    if (!value) return;

    const date = new Date(`${value}T00:00:00`);

    if (!isNaN(date.getTime())) {
      this.store.setDate(date);
    }
  }

  protected onMonthChange(value: string): void {
    if (!value) return;

    const [year, month] = value.split('-').map(Number);

    if (!Number.isFinite(year) || !Number.isFinite(month)) return;

    const date = new Date(this.store.selectedDate());
    date.setDate(1);
    date.setFullYear(year);
    date.setMonth(month - 1);
    this.store.setDate(date);
  }

  protected onYearChange(value: string | number): void {
    const year = typeof value === 'string' ? Number(value) : value;

    if (!Number.isFinite(year) || year < 1900 || year > 2100) return;

    const date = new Date(this.store.selectedDate());
    date.setFullYear(year);
    this.store.setDate(date);
  }
}
