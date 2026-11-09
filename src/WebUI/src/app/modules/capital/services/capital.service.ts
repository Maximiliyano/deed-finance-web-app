import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, tap, throwError } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { AddCapitalRequest } from '../models/add-capital-request';
import { UpdateCapitalRequest } from '../models/update-capital-request';
import { CapitalResponse } from '../models/capital-response';
import { UpdateCapitalOrderRequest } from '../models/update-capital-order-request';
import { QueryParams } from '../../../core/models/query-params';

@Injectable({
  providedIn: 'root'
})
export class CapitalService {
  private readonly httpClient = inject(HttpClient);

  private baseApiUrl = '/api/capitals';

  private readonly state$ = new BehaviorSubject<CapitalResponse[]>([]);
  readonly capitals$ = this.state$.asObservable();

  get current(): CapitalResponse[] {
    return this.state$.value;
  }

  load(params: QueryParams): Observable<CapitalResponse[]> {
    const isFiltered = !!params.searchTerm || !!params.filterBy;

    return this.httpClient.post<CapitalResponse[]>(`${this.baseApiUrl}/all`, params)
      .pipe(tap(items => {
        if (!isFiltered) {
          this.state$.next(items);
        }
      }));
  }

  refresh(params: QueryParams = { searchTerm: null, sortBy: null, sortDirection: null, filterBy: null }): void {
    this.load(params).subscribe();
  }

  getById(id: number): Observable<CapitalResponse> {
    return this.httpClient.get<CapitalResponse>(`${this.baseApiUrl}/${id}`);
  }

  create(request: AddCapitalRequest): Observable<number> {
    const tempId = -Date.now();
    const optimistic = {
      id: tempId,
      name: request.name,
      balance: request.balance,
      currency: request.currency,
      onlyForSavings: request.onlyForSavings,
      includeInTotal: request.includeInTotal,
      totalIncome: 0,
      totalExpense: 0,
      totalTransferIn: 0,
      totalTransferOut: 0,
      createdAt: new Date(),
      createdBy: null
    } as CapitalResponse;

    const previous = this.state$.value;
    this.state$.next([...previous, optimistic]);

    return this.httpClient.post<number>(this.baseApiUrl, request).pipe(
      tap(realId => {
        this.state$.next(this.state$.value.map(c => c.id === tempId ? { ...c, id: realId } : c));
        this.refresh();
      }),
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  update(id: number, request: UpdateCapitalRequest): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.map(c => c.id === id
      ? { ...c, name: (request).name ?? c.name, balance: (request).balance ?? c.balance, includeInTotal: (request).includeInTotal ?? c.includeInTotal, onlyForSavings: (request).onlyForSavings ?? c.onlyForSavings }
      : c));
    return this.httpClient.put<void>(`${this.baseApiUrl}/${id}`, request).pipe(
      tap(() => this.refresh()),
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  updateOrder(request: UpdateCapitalOrderRequest): Observable<void> {
    const previous = this.state$.value;
    const orderMap = new Map(request.capitals.map(o => [o.id, o.orderIndex]));
    const indexFallback = new Map(previous.map((c, i) => [c.id, i]));
    this.state$.next(
      [...previous].sort((a, b) => {
        const ai = orderMap.get(a.id) ?? indexFallback.get(a.id) ?? 0;
        const bi = orderMap.get(b.id) ?? indexFallback.get(b.id) ?? 0;
        return ai - bi;
      })
    );
    return this.httpClient.put<void>(`${this.baseApiUrl}/orders`, request).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  patchSavingsOnly(id: number, value: boolean): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.map(c => c.id === id ? { ...c, onlyForSavings: value } : c));
    return this.httpClient.patch<void>(`${this.baseApiUrl}/${id}/savings-only`, value).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  patchIncludeTotal(id: number, value: boolean): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.map(c => c.id === id ? { ...c, includeInTotal: value } : c));
    return this.httpClient.patch<void>(`${this.baseApiUrl}/${id}/include-in-total`, value).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  delete(id: number): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.filter(c => c.id !== id));
    return this.httpClient.delete<void>(`${this.baseApiUrl}/${id}`).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }
}
