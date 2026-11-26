import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, finalize, of, switchMap, tap } from 'rxjs';
import { PanelStateComponent } from '../../../../shared/components/panel-state/panel-state.component';
import { SharedModule } from '../../../../shared/shared.module';
import {
  BANK_TYPE_LABELS,
  BankProvider,
  BankSummaryCurrency,
  BankSummaryPeriod,
  BankSummaryType,
  BankTransactionType
} from '../../models/bank.model';
import { BankTransactionService } from '../../services/bank-transaction.service';
import { formatMoney } from '../../utils/bank-format';
import { CollapsibleComponent } from '../collapsible/collapsible.component';

const PERIODS: { value: BankSummaryPeriod; label: string }[] = [
  { value: 'day', label: 'Days' },
  { value: 'month', label: 'Months' },
  { value: 'year', label: 'Years' }
];

const DATE_FORMATS: Record<string, string> = {
  day: 'dd MMM yyyy',
  month: 'MMMM yyyy',
  year: 'yyyy'
};

@Component({
  selector: 'app-bank-summary',
  templateUrl: './bank-summary.component.html',
  styleUrl: './bank-summary.component.scss',
  standalone: true,
  imports: [SharedModule, PanelStateComponent, CollapsibleComponent],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BankSummaryComponent {
  private readonly transactions = inject(BankTransactionService);

  readonly connectionId = input<number | null>(null);
  readonly provider = input<BankProvider | null>(null);
  readonly version = input(0);
  readonly currency = input<string | null>(null);
  readonly currenciesChange = output<string[]>();

  protected readonly periods = PERIODS;
  protected readonly period = signal<BankSummaryPeriod>('month');
  protected readonly loading = signal(false);
  protected readonly showDetails = signal(false);

  protected readonly data = toSignal(
    toObservable(
      computed(() => ({
        period: this.period(),
        connectionId: this.connectionId(),
        provider: this.provider(),
        version: this.version()
      }))
    ).pipe(
      tap(() => this.loading.set(true)),
      switchMap(({ period, connectionId, provider }) =>
        this.transactions.summary(period, connectionId, provider).pipe(
          catchError(() => of(null)),
          finalize(() => this.loading.set(false))
        )
      ),
      tap(data => this.currenciesChange.emit(data?.currencies.map(item => item.currency) ?? []))
    ),
    { initialValue: null }
  );

  protected readonly current = computed(() => {
    const list = this.data()?.currencies ?? [];

    return list.find(item => item.currency === this.currency()) ?? list[0] ?? null;
  });
  protected readonly latest = computed(() => this.current()?.periods[0] ?? null);
  protected readonly dateFormat = computed(() => DATE_FORMATS[this.data()?.period.toLowerCase() ?? 'month']);
  protected readonly unit = computed(() => PERIODS.find(item => item.value === this.period())?.label.toLowerCase());

  protected setPeriod(period: BankSummaryPeriod): void {
    this.period.set(period);
  }

  protected toggleDetails(): void {
    this.showDetails.update(value => !value);
  }

  protected money(amount: number, currency: string): string {
    return formatMoney(amount, currency);
  }

  protected typesOf(currency: BankSummaryCurrency, direction: BankSummaryType['direction']): BankSummaryType[] {
    return currency.types.filter(item => item.direction === direction);
  }

  protected typeLabel(type: string): string {
    return BANK_TYPE_LABELS[type as BankTransactionType] || type;
  }

  protected share(amount: number, total: number): number {
    return total > 0 ? Math.round((amount / total) * 100) : 0;
  }
}
