import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SharedModule } from '../../../../shared/shared.module';
import { BANK_TYPE_LABELS, BankNotification, BankTransactionType } from '../../models/bank.model';
import { BankConnectionService } from '../../services/bank-connection.service';
import { BankNotificationService } from '../../services/bank-notification.service';

type BellFilter = 'all' | Extract<BankTransactionType, 'TopUp' | 'Withdrawal' | 'Transfer'>;

const FILTERS: { value: BellFilter; label: string }[] = [
  { value: 'all', label: 'All' },
  { value: 'TopUp', label: 'Top-ups' },
  { value: 'Withdrawal', label: 'Withdrawals' },
  { value: 'Transfer', label: 'Transfers' }
];

const TYPE_ICONS: Record<BankTransactionType, string> = {
  None: 'fa-circle-info',
  TopUp: 'fa-arrow-down',
  Withdrawal: 'fa-money-bill-wave',
  Transfer: 'fa-right-left',
  Payment: 'fa-credit-card'
};

@Component({
  selector: 'app-notification-bell',
  templateUrl: './notification-bell.component.html',
  styleUrl: './notification-bell.component.scss',
  standalone: true,
  imports: [SharedModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotificationBellComponent implements OnInit {
  private readonly service = inject(BankNotificationService);
  private readonly connectionService = inject(BankConnectionService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly filters = FILTERS;
  protected readonly unread = this.service.unreadCount;
  protected readonly connected = toSignal(this.connectionService.connections$.pipe(map(c => c.length > 0)), {
    initialValue: false
  });
  protected readonly open = signal(false);
  protected readonly filter = signal<BellFilter>('all');

  protected readonly items = computed(() => {
    const filter = this.filter();
    const items = this.service.items();

    return filter === 'all' ? items : items.filter(item => item.type === filter);
  });

  ngOnInit(): void {
    this.connectionService.load().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
    this.service.connect(this.destroyRef);
    this.destroyRef.onDestroy(() => this.service.reset());
  }

  protected toggle(event: MouseEvent): void {
    event.stopPropagation();
    this.open.update(value => !value);
  }

  protected close(): void {
    this.open.set(false);
  }

  protected setFilter(filter: BellFilter): void {
    this.filter.set(filter);
  }

  protected read(item: BankNotification): void {
    if (!item.isRead) {
      this.service.markRead([item.id]);
    }
  }

  protected markAllRead(): void {
    this.service.markRead();
  }

  protected icon(item: BankNotification): string {
    return item.kind === 'ConnectionIssue' ? 'fa-triangle-exclamation' : TYPE_ICONS[item.type];
  }

  protected title(item: BankNotification): string {
    if (item.kind === 'ConnectionIssue') return `${item.accountLabel ?? 'Privat24'} needs attention`;
    if (item.counterparty) return item.counterparty;

    return BANK_TYPE_LABELS[item.type] || (item.direction === 'Incoming' ? 'Incoming' : 'Outgoing');
  }

  protected subtitle(item: BankNotification): string {
    const parts = [BANK_TYPE_LABELS[item.type], item.accountLabel].filter(Boolean);

    return parts.join(' · ');
  }
}
