import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { DestroyRef, Injectable, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EMPTY, Observable, catchError, filter, fromEvent, merge, switchMap, tap, timer } from 'rxjs';
import { SKIP_ERROR_TOAST } from '../../../core/interceptors/error.interceptor';
import { PopupMessageService } from '../../../shared/services/popup-message.service';
import { BankNotification, BankNotifications } from '../models/bank.model';

const POLL_INTERVAL_MS = 60_000;
const MAX_INDIVIDUAL_TOASTS = 3;
const EMPTY_STATE: BankNotifications = { unreadCount: 0, items: [] };

const money = new Intl.NumberFormat(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

export function describeBankNotification(notification: BankNotification): string {
  if (notification.kind === 'ConnectionIssue') {
    return 'Privat24 connection needs attention. Reconnect it in your profile.';
  }

  const sign = notification.direction === 'Incoming' ? '+' : '-';
  const amount = `${sign}${money.format(notification.amount)} ${notification.currency ?? ''}`.trim();

  return notification.counterparty ? `${amount} - ${notification.counterparty}` : amount;
}

@Injectable({ providedIn: 'root' })
export class BankNotificationService {
  private readonly http = inject(HttpClient);
  private readonly popup = inject(PopupMessageService);
  private readonly baseUrl = '/api/bank-notifications';
  private readonly silent = new HttpContext().set(SKIP_ERROR_TOAST, true);

  private readonly state = signal<BankNotifications>(EMPTY_STATE);
  private readonly seen = new Set<number>();
  private primed = false;

  readonly items = computed(() => this.state().items);
  readonly unreadCount = computed(() => this.state().unreadCount);

  poll(destroyRef: DestroyRef): void {
    merge(timer(0, POLL_INTERVAL_MS), fromEvent(document, 'visibilitychange'))
      .pipe(
        filter(() => !document.hidden),
        switchMap(() => this.load().pipe(catchError(() => EMPTY))),
        takeUntilDestroyed(destroyRef)
      )
      .subscribe();
  }

  load(): Observable<BankNotifications> {
    const params = new HttpParams().set('limit', 30);

    return this.http
      .get<BankNotifications>(this.baseUrl, { params, context: this.silent })
      .pipe(
        tap(response => {
          this.announce(response);
          this.state.set(response);
        })
      );
  }

  markRead(ids?: number[]): void {
    const idSet = ids ? new Set(ids) : null;
    const previous = this.state();
    const items = previous.items.map(item => (!idSet || idSet.has(item.id) ? { ...item, isRead: true } : item));

    this.state.set({ items, unreadCount: items.filter(item => !item.isRead).length });

    this.http.post<void>(`${this.baseUrl}/read`, { ids: ids ?? null }, { context: this.silent }).subscribe({
      error: () => this.state.set(previous)
    });
  }

  reset(): void {
    this.state.set(EMPTY_STATE);
    this.seen.clear();
    this.primed = false;
  }

  private announce(response: BankNotifications): void {
    const fresh = response.items.filter(item => !item.isRead && !this.seen.has(item.id));
    response.items.forEach(item => this.seen.add(item.id));

    if (!this.primed) {
      this.primed = true;
      return;
    }

    if (fresh.length > MAX_INDIVIDUAL_TOASTS) {
      this.popup.info(`${fresh.length} new bank transactions`, 6000);
      return;
    }

    for (const item of fresh) {
      if (item.kind === 'ConnectionIssue') {
        this.popup.warning(describeBankNotification(item), 8000);
      } else {
        this.popup.info(describeBankNotification(item), 6000);
      }
    }
  }
}
