import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { DestroyRef, Injectable, computed, inject, signal } from '@angular/core';
import { HttpTransportType, HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { EMPTY, Observable, Subscription, catchError, filter, fromEvent, switchMap, tap, timer } from 'rxjs';
import { SKIP_ERROR_TOAST } from '../../../core/interceptors/error.interceptor';
import { PopupMessageService } from '../../../shared/services/popup-message.service';
import { BANK_TYPE_LABELS, BankNotification, BankNotifications } from '../models/bank.model';

const HUB_URL = '/api/hubs/notifications';
const HUB_EVENT = 'bankNotifications';
const FALLBACK_POLL_INTERVAL_MS = 300_000;
const RECONNECT_DELAYS_MS = [0, 2_000, 10_000, 30_000, 60_000];
const MAX_INDIVIDUAL_TOASTS = 3;
const MAX_ITEMS = 30;
const EMPTY_STATE: BankNotifications = { unreadCount: 0, items: [] };

const money = new Intl.NumberFormat(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

export function describeBankNotification(notification: BankNotification): string {
  const account = notification.accountLabel ?? 'Your bank';

  if (notification.kind === 'ConnectionIssue') {
    return `${account} connection needs attention. Reconnect it in your profile.`;
  }

  const sign = notification.direction === 'Incoming' ? '+' : '-';
  const amount = `${sign}${money.format(notification.amount)} ${notification.currency ?? ''}`.trim();
  const type = BANK_TYPE_LABELS[notification.type];
  const counterparty = notification.counterparty ? ` - ${notification.counterparty}` : '';
  const source = notification.accountLabel ? ` (${notification.accountLabel})` : '';

  return `${type ? `${type}: ` : ''}${amount}${counterparty}${source}`;
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
  private hub: HubConnection | null = null;
  private fallback: Subscription | null = null;
  private visibility: Subscription | null = null;

  readonly items = computed(() => this.state().items);
  readonly unreadCount = computed(() => this.state().unreadCount);

  connect(destroyRef: DestroyRef): void {
    this.reset();

    this.reload();

    this.visibility = fromEvent(document, 'visibilitychange')
      .pipe(filter(() => !document.hidden))
      .subscribe(() => this.reload());

    this.startHub(destroyRef);
  }

  load(): Observable<BankNotifications> {
    const params = new HttpParams().set('limit', MAX_ITEMS);

    return this.http.get<BankNotifications>(this.baseUrl, { params, context: this.silent }).pipe(
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
    this.fallback?.unsubscribe();
    this.visibility?.unsubscribe();
    this.fallback = null;
    this.visibility = null;

    const hub = this.hub;
    this.hub = null;
    void hub?.stop();

    this.state.set(EMPTY_STATE);
    this.seen.clear();
    this.primed = false;
  }

  private startHub(destroyRef: DestroyRef): void {
    const hub = new HubConnectionBuilder()
      .withUrl(HUB_URL, { transport: HttpTransportType.WebSockets, skipNegotiation: true })
      .withAutomaticReconnect(RECONNECT_DELAYS_MS)
      .configureLogging(LogLevel.None)
      .build();

    hub.on(HUB_EVENT, (items: BankNotification[]) => this.receive(items));
    hub.onreconnected(() => this.reload());
    hub.onclose(() => {
      if (this.hub === hub) {
        this.startFallback(destroyRef);
      }
    });

    this.hub = hub;

    hub.start().catch(() => {
      if (this.hub === hub) {
        this.startFallback(destroyRef);
      }
    });
  }

  private startFallback(destroyRef: DestroyRef): void {
    if (this.fallback) return;

    this.fallback = timer(FALLBACK_POLL_INTERVAL_MS, FALLBACK_POLL_INTERVAL_MS)
      .pipe(
        filter(() => !document.hidden),
        switchMap(() => this.load().pipe(catchError(() => EMPTY)))
      )
      .subscribe();

    destroyRef.onDestroy(() => this.fallback?.unsubscribe());
  }

  private reload(): void {
    this.load()
      .pipe(catchError(() => EMPTY))
      .subscribe();
  }

  private receive(incoming: BankNotification[]): void {
    const fresh = incoming.filter(item => !this.seen.has(item.id));

    if (fresh.length === 0) return;

    fresh.forEach(item => this.seen.add(item.id));

    const current = this.state();
    const items = [...fresh, ...current.items]
      .sort((a, b) => b.occurredAt.localeCompare(a.occurredAt) || b.id - a.id)
      .slice(0, MAX_ITEMS);

    this.state.set({
      items,
      unreadCount: current.unreadCount + fresh.filter(item => !item.isRead).length
    });

    this.toast(fresh.filter(item => !item.isRead));
  }

  private announce(response: BankNotifications): void {
    const fresh = response.items.filter(item => !item.isRead && !this.seen.has(item.id));
    response.items.forEach(item => this.seen.add(item.id));

    if (!this.primed) {
      this.primed = true;
      return;
    }

    this.toast(fresh);
  }

  private toast(fresh: BankNotification[]): void {
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
