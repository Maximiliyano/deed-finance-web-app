import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { EMPTY, Subject, catchError, filter, finalize, interval, switchMap, tap } from 'rxjs';
import { DialogService } from '../../../../shared/components/dialogs/services/dialog.service';
import { PanelStateComponent } from '../../../../shared/components/panel-state/panel-state.component';
import { SharedModule } from '../../../../shared/shared.module';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import {
  BANK_TYPE_LABELS,
  BankHistoryImportResponse,
  BankImportStatus,
  BankNotification,
  BankTransactionsPage,
  BankTransactionType
} from '../../models/bank.model';
import { BankConnectionService } from '../../services/bank-connection.service';
import { BankTransactionService } from '../../services/bank-transaction.service';
import { ImportHistoryDialogComponent } from '../import-history-dialog/import-history-dialog.component';
import { ImportTransactionDialogComponent } from '../import-transaction-dialog/import-transaction-dialog.component';

type TypeFilter = 'all' | 'review' | Exclude<BankTransactionType, 'None'>;

const FILTERS: { value: TypeFilter; label: string }[] = [
  { value: 'all', label: 'All' },
  { value: 'review', label: 'To review' },
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
  private readonly dialogService = inject(DialogService);
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

  protected readonly backfilling = computed(() => this.connections().filter(c => c.backfillCursor !== null));
  protected readonly backfillFrom = computed(
    () =>
      this.backfilling()
        .map(c => c.backfillFrom)
        .filter((value): value is string => value !== null)
        .sort()[0] ?? null
  );

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
          this.transactions.loadPage(this.page(), this.pageSize(), this.typeParam(), this.statusParam()).pipe(
            catchError(() => EMPTY),
            finalize(() => this.loading.set(false))
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(data => this.data.set(data));

    interval(60_000)
      .pipe(
        filter(() => this.backfilling().length > 0),
        switchMap(() => this.connectionService.load()),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.goTo(this.page()));

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
    this.dialogService
      .open(ImportHistoryDialogComponent, {})
      .afterClosed$.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((result: BankHistoryImportResponse | null) => {
        if (!result) return;

        this.popup.info(
          result.inProgress
            ? 'History import started. Transactions will appear in the background, about one month per minute.'
            : 'No history to import. Connect a bank account first.'
        );

        this.connectionService.load().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
        this.goTo(1);
      });
  }

  protected add(item: BankNotification): void {
    const connection = this.connections().find(c => c.id === item.bankConnectionId);

    this.dialogService
      .open(ImportTransactionDialogComponent, { data: { notification: item, connection } })
      .afterClosed$.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((added: boolean) => {
        if (added) {
          this.goTo(this.page());
        }
      });
  }

  protected skip(item: BankNotification): void {
    if (this.busy()) return;

    this.busy.set(true);

    this.transactions
      .dismiss(item.id)
      .pipe(
        finalize(() => this.busy.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.goTo(this.page()));
  }

  protected icon(item: BankNotification): string {
    return TYPE_ICONS[item.type];
  }

  protected typeLabel(item: BankNotification): string {
    return BANK_TYPE_LABELS[item.type] || (item.direction === 'Incoming' ? 'Incoming' : 'Outgoing');
  }

  private typeParam(): BankTransactionType | null {
    const filter = this.filter();

    return filter === 'all' || filter === 'review' ? null : filter;
  }

  private statusParam(): BankImportStatus | null {
    return this.filter() === 'review' ? 'Pending' : null;
  }
}
