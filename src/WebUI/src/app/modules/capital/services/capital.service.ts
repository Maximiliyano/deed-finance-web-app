import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, catchError, shareReplay, tap, throwError } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { AddCapitalRequest } from '../models/add-capital-request';
import { UpdateCapitalRequest } from '../models/update-capital-request';
import { CapitalResponse } from '../models/capital-response';
import { UpdateCapitalOrderRequest } from '../models/update-capital-order-request';
import { QueryParams } from '../../../core/models/query-params';
import { CurrencyType } from '../../../core/types/currency-type';
import { convertCurrency } from '../../../shared/utils/currency-conversion.util';
import { Exchange } from '../../../core/models/exchange-model';

@Injectable({
  providedIn: 'root'
})
export class CapitalService {
  private baseApiUrl = environment.apiUrl + '/api/capitals';

  private readonly state$ = new BehaviorSubject<CapitalResponse[]>([]);
  readonly capitals$ = this.state$.asObservable();

  constructor(private readonly httpClient: HttpClient) { }

  get current(): CapitalResponse[] {
    return this.state$.value;
  }

  getCurrency(capitalId: number): string | null {
    if (capitalId == null) return null;
    const cap = this.current.find(c => c.id === capitalId);
    return cap?.currency ?? null;
  }

  getTotalAmount(currency: string, exchanges: Exchange[]): number {
    return this.current
      .filter(c => c.includeInTotal)
      .reduce((sum, c) => sum + convertCurrency(c.balance, c.currency, currency, exchanges), 0);
  }

  load(params: QueryParams): Observable<CapitalResponse[]> {
    return this.httpClient.post<CapitalResponse[]>(`${this.baseApiUrl}/all`, params, { withCredentials: true })
      .pipe(
        shareReplay({
          bufferSize: 1,
          refCount: false
        }),
        tap(items => this.state$.next(items)));
  }

  refresh(params: QueryParams = { searchTerm: null, sortBy: null, sortDirection: null, filterBy: null }): void {
    this.load(params).subscribe();
  }

  getAll(params: QueryParams): Observable<CapitalResponse[]> {
    return this.load(params);
  }

  getById(id: number): Observable<CapitalResponse> {
    return this.httpClient.get<CapitalResponse>(`${this.baseApiUrl}/${id}`, { withCredentials: true });
  }

  create(request: AddCapitalRequest): Observable<number> {
    const tempId = -Date.now();
    const optimistic = {
      id: tempId,
      name: request.name,
      balance: request.balance,
      currency: CurrencyType[request.currency] as string,
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

    return this.httpClient.post<number>(this.baseApiUrl, request, { withCredentials: true }).pipe(
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
    return this.httpClient.put<void>(`${this.baseApiUrl}/${id}`, request, { withCredentials: true }).pipe(
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
    return this.httpClient.put<void>(`${this.baseApiUrl}/orders`, request, { withCredentials: true }).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  patchSavingsOnly(id: number, value: boolean): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.map(c => c.id === id ? { ...c, onlyForSavings: value } : c));
    return this.httpClient.patch<void>(`${this.baseApiUrl}/${id}/savings-only`, value, { withCredentials: true }).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  patchIncludeTotal(id: number, value: boolean): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.map(c => c.id === id ? { ...c, includeInTotal: value } : c));
    return this.httpClient.patch<void>(`${this.baseApiUrl}/${id}/include-in-total`, value, { withCredentials: true }).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  delete(id: number): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.filter(c => c.id !== id));
    return this.httpClient.delete<void>(`${this.baseApiUrl}/${id}`, { withCredentials: true }).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }
}
