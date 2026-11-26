import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { EMPTY, Subject, catchError, finalize, switchMap, tap } from 'rxjs';
import { PanelStateComponent } from '../../../../shared/components/panel-state/panel-state.component';
import { SharedModule } from '../../../../shared/shared.module';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { BANK_TYPE_LABELS, BankNotification, BankTransactionsPage, BankTransactionType } from '../../models/bank.model';
import { BankConnectionService } from '../../services/bank-connection.service';
import { BankTransactionService } from '../../services/bank-transaction.service';

type TypeFilter = 'all' | Exclude<BankTransactionType, 'None'>;

const FILTERS: { value: TypeFilter; label: string }[] = [
  { value: 'all', label: 'All' },
  { value: 'TopUp', label: 'Top-ups' },
  { value: 'Withdrawal', label: 'Withdrawals' },
  { value: 'Transfer', label: 'Transfers' },
  { value: 'Payment', label: 'Payments' }
];

const TYPE_ICONS: Record<BankTransactionType, string> = {
  None: 'fa-circle-info',
  TopUp: 'fa-arrow-down',
  Withdrawal: 'fa-money-bill-wave',
  Transfer: 'fa-right-left',
  Payment: 'fa-credit-card'
};

@Component({
  selector: 'app-bank-transactions',
  templateUrl: './bank-transactions.component.html',
  styleUrl: './bank-transactions.component.scss',
  standalone: true,
  imports: [SharedModule, PanelStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BankTransactionsComponent implements OnInit {
  private readonly transactions = inject(BankTransactionService);
  private readonly connectionService = inject(BankConnectionService);
  private readonly popup = inject(PopupMessageService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly reload$ = new Subject<void>();

  protected readonly filters = FILTERS;
  protected readonly pageSizes = [10, 20, 50];
  protected readonly connections = toSignal(this.connectionService.connections$, { initialValue: [] });

  protected readonly filter = signal<TypeFilter>('all');
  protected readonly page = signal(1);
  protected readonly pageSize = signal(20);
  protected readonly data = signal<BankTransactionsPage | null>(null);
  protected readonly loading = signal(false);
  protected readonly busy = signal(false);

  protected readonly items = computed(() => this.data()?.items ?? []);
  protected readonly total = computed(() => this.data()?.total ?? 0);
  protected readonly totalPages = computed(() => Math.max(1, Math.ceil(this.total() / this.pageSize())));
  protected readonly rangeStart = computed(() => (this.total() === 0 ? 0 : (this.page() - 1) * this.pageSize() + 1));
  protected readonly rangeEnd = computed(() => Math.min(this.page() * this.pageSize(), this.total()));

  ngOnInit(): void {
    document.title = 'Deed - Bank transactions';

    this.connectionService.load().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();

    this.reload$
      .pipe(
        tap(() => this.loading.set(true)),
        switchMap(() =>
          this.transactions.loadPage(this.page(), this.pageSize(), this.typeParam()).pipe(
            catchError(() => EMPTY),
            finalize(() => this.loading.set(false))
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(data => this.data.set(data));

    this.reload$.next();
  }

  protected setFilter(filter: TypeFilter): void {
    this.filter.set(filter);
    this.goTo(1);
  }

  protected setPageSize(value: string): void {
    this.pageSize.set(Number(value));
    this.goTo(1);
  }

  protected goTo(page: number): void {
    this.page.set(Math.min(Math.max(page, 1), this.totalPages()));
    this.reload$.next();
  }

  protected refresh(): void {
    if (this.busy()) return;

    this.busy.set(true);

    this.connectionService
      .syncNow()
      .pipe(
        finalize(() => this.busy.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(created => {
        if (created > 0) {
          this.popup.info(`${created} new transaction${created === 1 ? '' : 's'} found`);
        }

        this.goTo(1);
      });
  }

  protected importHistory(): void {
    if (this.busy()) return;

    this.busy.set(true);

    this.connectionService
      .importHistory()
      .pipe(
        finalize(() => this.busy.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(imported => {
        this.popup.info(
          imported > 0 ? `${imported} transaction${imported === 1 ? '' : 's'} imported` : 'No new transactions found'
        );
        this.goTo(1);
      });
  }

  protected icon(item: BankNotification): string {
    return TYPE_ICONS[item.type];
  }

  protected typeLabel(item: BankNotification): string {
    return BANK_TYPE_LABELS[item.type] || (item.direction === 'Incoming' ? 'Incoming' : 'Outgoing');
  }

  private typeParam(): BankTransactionType | null {
    const filter = this.filter();

    return filter === 'all' ? null : filter;
  }
}
