import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, tap, throwError } from 'rxjs';
import { Debt } from '../models/debt.model';
import { CreateDebtRequest } from '../models/debt-request-create';
import { UpdateDebtRequest } from '../models/debt-request-update';

@Injectable({ providedIn: 'root' })
export class DebtService {
  private readonly http = inject(HttpClient);

  private readonly baseUrl = '/api/debts';

  private readonly state$ = new BehaviorSubject<Debt[]>([]);
  readonly debts$ = this.state$.asObservable();

  get current(): Debt[] { return this.state$.value; }

  load(): Observable<Debt[]> {
    return this.http.get<Debt[]>(this.baseUrl)
      .pipe(tap(items => this.state$.next(items)));
  }

  refresh(): void { this.load().subscribe(); }

  create(request: CreateDebtRequest): Observable<number> {
    const tempId = -Date.now();
    const previous = this.state$.value;
    const optimistic: Debt = {
      id: tempId,
      item: request.item,
      amount: request.amount,
      currency: request.currency,
      source: request.source,
      recipient: request.recipient,
      borrowedAt: request.borrowedAt,
      deadlineAt: request.deadlineAt,
      note: request.note,
      isPaid: false,
      capitalId: request.capitalId,
      capitalName: null,
      orderIndex: previous.length
    };
    this.state$.next([...previous, optimistic]);

    return this.http.post<number>(this.baseUrl, request).pipe(
      tap(realId => {
        this.state$.next(this.state$.value.map(d => d.id === tempId ? { ...d, id: realId } : d));
        this.refresh();
      }),
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  update(id: number, request: UpdateDebtRequest): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.map(d => d.id === id
      ? { ...d, ...request, currency: request.currency.toString(), capitalId: request.payFromCapitalId ?? d.capitalId }
      : d));
    return this.http.put<void>(`${this.baseUrl}/${id}`, request).pipe(
      tap(() => this.refresh()),
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  delete(id: number): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.filter(d => d.id !== id));
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  updateOrder(debts: { id: number; orderIndex: number }[]): Observable<void> {
    const previous = this.state$.value;
    const orderMap = new Map(debts.map(o => [o.id, o.orderIndex]));
    this.state$.next(
      [...previous].sort((a, b) =>
        (orderMap.get(a.id) ?? a.orderIndex) - (orderMap.get(b.id) ?? b.orderIndex)
      )
    );
    return this.http.put<void>(`${this.baseUrl}/orders`, { debts }).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }
}
