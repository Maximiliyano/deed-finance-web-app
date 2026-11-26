import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { SharedModule } from '../../../../shared/shared.module';
import { BankNotification } from '../../models/bank.model';
import { BankNotificationService } from '../../services/bank-notification.service';

@Component({
  selector: 'app-notification-bell',
  templateUrl: './notification-bell.component.html',
  styleUrl: './notification-bell.component.scss',
  standalone: true,
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotificationBellComponent implements OnInit {
  private readonly service = inject(BankNotificationService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly items = this.service.items;
  protected readonly unread = this.service.unreadCount;
  protected readonly open = signal(false);

  ngOnInit(): void {
    this.service.poll(this.destroyRef);
    this.destroyRef.onDestroy(() => this.service.reset());
  }

  protected toggle(event: MouseEvent): void {
    event.stopPropagation();
    this.open.update(value => !value);
  }

  protected close(): void {
    this.open.set(false);
  }

  protected read(item: BankNotification): void {
    if (!item.isRead) {
      this.service.markRead([item.id]);
    }
  }

  protected markAllRead(): void {
    this.service.markRead();
  }

  protected title(item: BankNotification): string {
    if (item.kind === 'ConnectionIssue') return 'Privat24 needs attention';
    if (item.counterparty) return item.counterparty;

    return item.direction === 'Incoming' ? 'Incoming transfer' : 'Outgoing transfer';
  }
}
