import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { SharedModule } from '../../../../shared/shared.module';
import { PeriodFilter, PeriodState } from '../../models/period-state';

const pad = (value: number): string => String(value).padStart(2, '0');
const toDateInput = (date: Date): string => `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;

@Component({
  selector: 'app-period-bar',
  templateUrl: './period-bar.component.html',
  styleUrl: './period-bar.component.scss',
  standalone: true,
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PeriodBarComponent {
  readonly state = input.required<PeriodState>();
  readonly flush = input(false);
  readonly allowAll = input(false);

  protected readonly periods: { key: PeriodFilter; label: string }[] = [
    { key: 'day', label: 'Day' },
    { key: 'month', label: 'Month' },
    { key: 'year', label: 'Year' },
    { key: 'custom', label: 'Custom' }
  ];

  protected readonly selectedDay = computed(() => toDateInput(this.state().date()));

  protected readonly selectedMonth = computed(() => {
    const d = this.state().date();
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}`;
  });

  protected readonly selectedYear = computed(() => this.state().date().getFullYear());
  protected readonly customFrom = computed(() => toDateInput(this.state().customFrom()));
  protected readonly customTo = computed(() => toDateInput(this.state().customTo()));

  protected readonly selectedPeriodLabel = computed(() => {
    const state = this.state();
    const filter = state.filter();

    if (filter === 'custom') return state.customLabel();

    const options: Intl.DateTimeFormatOptions =
      filter === 'day'
        ? { year: 'numeric', month: 'short', day: 'numeric' }
        : filter === 'year'
          ? { year: 'numeric' }
          : { year: 'numeric', month: 'long' };

    return state.date().toLocaleDateString(undefined, options);
  });

  protected onDayChange(value: string): void {
    const date = this.parseDate(value);

    if (date) {
      this.state().setDate(date);
    }
  }

  protected onMonthChange(value: string): void {
    if (!value) return;

    const [year, month] = value.split('-').map(Number);

    if (!Number.isFinite(year) || !Number.isFinite(month)) return;

    this.state().setDate(new Date(year, month - 1, 1));
  }

  protected onYearChange(value: string | number): void {
    const year = typeof value === 'string' ? Number(value) : value;

    if (!Number.isFinite(year) || year < 1900 || year > 2100) return;

    const date = new Date(this.state().date());
    date.setFullYear(year);
    this.state().setDate(date);
  }

  protected onFromChange(value: string): void {
    const date = this.parseDate(value);

    if (date) {
      this.state().setCustomFrom(date);
    }
  }

  protected onToChange(value: string): void {
    const date = this.parseDate(value);

    if (date) {
      this.state().setCustomTo(date);
    }
  }

  private parseDate(value: string): Date | null {
    if (!value) return null;

    const date = new Date(`${value}T00:00:00`);

    return isNaN(date.getTime()) ? null : date;
  }
}
