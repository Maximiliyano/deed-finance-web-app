import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  effect,
  inject,
  signal
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { EMPTY, Subject, catchError, filter, finalize, interval, switchMap, tap, timer } from 'rxjs';
import { DialogService } from '../../../../shared/components/dialogs/services/dialog.service';
import { PanelStateComponent } from '../../../../shared/components/panel-state/panel-state.component';
import { SharedModule } from '../../../../shared/shared.module';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import {
  BANK_TYPE_LABELS,
  BankHistoryImportResponse,
  BankImportStatus,
  BankNotification,
  BankProvider,
  BankTransactionsPage,
  BankTransactionType
} from '../../models/bank.model';
import { BankConnectionService } from '../../services/bank-connection.service';
import { BankTransactionService } from '../../services/bank-transaction.service';
import { BankSummaryComponent } from '../bank-summary/bank-summary.component';
import { ImportHistoryDialogComponent } from '../import-history-dialog/import-history-dialog.component';
import { ImportTransactionDialogComponent } from '../import-transaction-dialog/import-transaction-dialog.component';

type TypeFilter = 'all' | 'review' | Exclude<BankTransactionType, 'None'>;

const FILTERS: { value: TypeFilter; label: string }[] = [
  { value: 'all', label: 'All' },
  { value: 'review', label: 'To review' },
  { value: 'TopUp', label: 'Top-ups' },
  { value: 'Withdrawal', label: 'Withdrawals' },
  { value: 'Transfer', label: 'Transfers' },
  { value: 'Payment', label: 'Payments' },
  { value: 'Exchange', label: 'Exchanges' }
];

const PROVIDER_OPTIONS: { value: BankProvider; label: string }[] = [
  { value: 'PrivatAutoclient', label: 'Privat24' },
  { value: 'Monobank', label: 'Monobank' }
];

const TYPE_ICONS: Record<BankTransactionType, string> = {
  None: 'fa-circle-info',
  TopUp: 'fa-arrow-down',
  Withdrawal: 'fa-money-bill-wave',
  Transfer: 'fa-right-left',
  Payment: 'fa-credit-card',
  Exchange: 'fa-arrows-rotate'
};

@Component({
  selector: 'app-bank-transactions',
  templateUrl: './bank-transactions.component.html',
  styleUrl: './bank-transactions.component.scss',
  standalone: true,
  imports: [SharedModule, PanelStateComponent, BankSummaryComponent],
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
  protected readonly pageSizes = [10, 20, 50, 100];
  private readonly allConnections = toSignal(this.connectionService.connections$, { initialValue: [] });
  protected readonly connections = computed(() => this.allConnections().filter(connection => !connection.isExcluded));

  protected readonly filter = signal<TypeFilter>('all');
  protected readonly accountId = signal<number | null>(null);
  protected readonly provider = signal<BankProvider | null>(null);

  protected readonly providers = computed(() =>
    PROVIDER_OPTIONS.filter(option => this.connections().some(connection => connection.provider === option.value))
  );
  protected readonly accountOptions = computed(() =>
    this.connections().filter(connection => !this.provider() || connection.provider === this.provider())
  );
  protected readonly page = signal(1);
  protected readonly pageSize = signal(20);
  protected readonly data = signal<BankTransactionsPage | null>(null);
  protected readonly loading = signal(false);
  protected readonly busy = signal(false);

  protected readonly backfilling = computed(() =>
    this.connections().filter(c => c.status === 'Active' && c.backfillCursor !== null && c.backfillFrom !== null)
  );
  protected readonly backfillFrom = computed(
    () =>
      this.backfilling()
        .map(c => c.backfillFrom!)
        .sort()[0] ?? null
  );
  protected readonly backfillCursor = computed(
    () =>
      this.backfilling()
        .map(c => c.backfillCursor!)
        .sort()
        .at(-1) ?? null
  );
  protected readonly backfillProgress = computed(() => {
    const now = Date.now();
    const ratios = this.backfilling().map(c => {
      const from = Date.parse(c.backfillFrom!);
      const cursor = Date.parse(c.backfillCursor!);
      const span = now - from;

      return span > 0 ? Math.min(Math.max((now - cursor) / span, 0), 1) : 0;
    });

    return ratios.length === 0 ? 0 : Math.round(Math.min(...ratios) * 100);
  });

  protected readonly items = computed(() => this.data()?.items ?? []);
  protected readonly total = computed(() => this.data()?.total ?? 0);
  protected readonly totalPages = computed(() => Math.max(1, Math.ceil(this.total() / this.pageSize())));
  protected readonly rangeStart = computed(() => (this.total() === 0 ? 0 : (this.page() - 1) * this.pageSize() + 1));
  protected readonly rangeEnd = computed(() => Math.min(this.page() * this.pageSize(), this.total()));

  protected readonly version = signal(0);
  protected readonly finished = signal(false);
  protected readonly eta = signal<string | null>(null);

  private wasImporting = false;
  private importStart: { at: number; progress: number } | null = null;

  constructor() {
    effect(() => {
      if (this.backfilling().length > 0) {
        this.wasImporting = true;
        return;
      }

      if (!this.wasImporting) return;

      this.wasImporting = false;
      this.importStart = null;
      this.eta.set(null);
      this.finished.set(true);
      this.version.update(value => value + 1);

      timer(5000)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => this.finished.set(false));
    });
  }

  ngOnInit(): void {
    document.title = 'Deed - Bank transactions';

    this.connectionService.load().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();

    this.reload$
      .pipe(
        tap(() => this.loading.set(true)),
        switchMap(() =>
          this.transactions
            .loadPage(
              this.page(),
              this.pageSize(),
              this.typeParam(),
              this.statusParam(),
              this.accountId(),
              this.provider()
            )
            .pipe(
              catchError(() => EMPTY),
              finalize(() => this.loading.set(false))
            )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(data => this.data.set(data));

    let lastCursor = this.backfillCursor();

    interval(4_000)
      .pipe(
        filter(() => this.backfilling().length > 0),
        switchMap(() => this.connectionService.load().pipe(catchError(() => EMPTY))),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        const cursor = this.backfillCursor();

        this.updateEta();

        if (cursor !== lastCursor) {
          lastCursor = cursor;
          this.version.update(value => value + 1);
          this.goTo(this.page());
        }
      });

    this.reload$.next();
  }

  protected setFilter(filter: TypeFilter): void {
    this.filter.set(filter);
    this.goTo(1);
  }

  protected setProvider(provider: BankProvider | null): void {
    this.provider.set(provider);

    if (!this.accountOptions().some(connection => connection.id === this.accountId())) {
      this.accountId.set(null);
    }

    this.goTo(1);
  }

  protected setAccount(value: string): void {
    this.accountId.set(value === '' ? null : Number(value));
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

  private updateEta(): void {
    const progress = this.backfillProgress();
    const now = Date.now();

    if (!this.importStart) {
      this.importStart = { at: now, progress };
      return;
    }

    const minutes = (now - this.importStart.at) / 60_000;
    const gained = progress - this.importStart.progress;

    if (minutes < 0.25 || gained < 1) {
      this.eta.set(null);
      return;
    }

    const left = Math.ceil(((100 - progress) / gained) * minutes);

    this.eta.set(left <= 1 ? 'Less than a minute left' : `About ${left} min left`);
  }

  protected cancelImport(): void {
    if (this.busy()) return;

    this.busy.set(true);

    this.connectionService
      .cancelHistory()
      .pipe(
        finalize(() => this.busy.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.wasImporting = false;
        this.importStart = null;
        this.eta.set(null);
        this.popup.info('History import cancelled. What was imported so far is kept.');
        this.goTo(this.page());
      });
  }

  protected importHistory(): void {
    this.dialogService
      .open(ImportHistoryDialogComponent, {})
      .afterClosed$.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((result: BankHistoryImportResponse | null) => {
        if (!result) return;

        if (!result.inProgress) {
          this.popup.info('No history to import. Connect a bank account first.');
        }

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
