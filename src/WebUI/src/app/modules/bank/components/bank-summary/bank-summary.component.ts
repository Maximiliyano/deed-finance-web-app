import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, finalize, of, switchMap, tap } from 'rxjs';
import { CURRENCY_SYMBOL } from '../../../../core/types/currency-type';
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

const NUMBER_FORMAT = new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

@Component({
  selector: 'app-bank-summary',
  templateUrl: './bank-summary.component.html',
  styleUrl: './bank-summary.component.scss',
  standalone: true,
  imports: [SharedModule, PanelStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BankSummaryComponent {
  private readonly transactions = inject(BankTransactionService);

  readonly connectionId = input<number | null>(null);
  readonly provider = input<BankProvider | null>(null);
  readonly version = input(0);

  protected readonly periods = PERIODS;
  protected readonly period = signal<BankSummaryPeriod>('month');
  protected readonly loading = signal(false);

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
      )
    ),
    { initialValue: null }
  );

  protected readonly currencies = computed(() => this.data()?.currencies ?? []);
  protected readonly dateFormat = computed(() => DATE_FORMATS[this.data()?.period.toLowerCase() ?? 'month']);
  protected readonly unit = computed(() => PERIODS.find(item => item.value === this.period())?.label.toLowerCase());

  protected setPeriod(period: BankSummaryPeriod): void {
    this.period.set(period);
  }

  protected money(amount: number, currency: string): string {
    const text = NUMBER_FORMAT.format(amount);
    const symbol = CURRENCY_SYMBOL[currency] ?? currency;

    return symbol ? `${symbol} ${text}` : text;
  }

  protected net(currency: BankSummaryCurrency): number {
    return currency.income - currency.expense;
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
