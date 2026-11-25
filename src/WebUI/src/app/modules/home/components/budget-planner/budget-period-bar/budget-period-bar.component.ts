import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { SharedModule } from '../../../../../shared/shared.module';
import { DashboardStore, PlanFilter } from '../../../services/dashboard-store.service';

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

  protected readonly periods: { key: PlanFilter; label: string }[] = [
    { key: 'day', label: 'Day' },
    { key: 'month', label: 'Month' },
    { key: 'year', label: 'Year' }
  ];

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
      this.store.planFilter() === 'day'
        ? { year: 'numeric', month: 'short', day: 'numeric' }
        : this.store.planFilter() === 'year'
          ? { year: 'numeric' }
          : { year: 'numeric', month: 'long' };

    return this.store.selectedDate().toLocaleDateString(undefined, options);
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
