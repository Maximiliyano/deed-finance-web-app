import { HttpClient, HttpContext } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, switchMap, tap } from 'rxjs';
import { SKIP_ERROR_TOAST } from '../../../core/interceptors/error.interceptor';
import {
  BankConnection,
  BankHistoryImportRequest,
  BankHistoryImportResponse,
  ConnectMonobankRequest,
  ConnectPrivatRequest,
  MonobankAccount,
  PrivatAccount,
  UpdateBankConnectionRequest
} from '../models/bank.model';

@Injectable({ providedIn: 'root' })
export class BankConnectionService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/bank-connections';

  private readonly state$ = new BehaviorSubject<BankConnection[]>([]);
  readonly connections$ = this.state$.asObservable();

  load(): Observable<BankConnection[]> {
    return this.http.get<BankConnection[]>(this.baseUrl).pipe(tap(connections => this.state$.next(connections)));
  }

  connectPrivat(request: ConnectPrivatRequest): Observable<BankConnection[]> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, true);

    return this.http.post<void>(`${this.baseUrl}/privat`, request, { context }).pipe(switchMap(() => this.load()));
  }

  privatAccounts(token: string): Observable<PrivatAccount[]> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, true);

    return this.http.post<PrivatAccount[]>(`${this.baseUrl}/privat/accounts`, { token }, { context });
  }

  monobankAccounts(token: string): Observable<MonobankAccount[]> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, true);

    return this.http.post<MonobankAccount[]>(`${this.baseUrl}/monobank/accounts`, { token }, { context });
  }

  connectMonobank(request: ConnectMonobankRequest): Observable<BankConnection[]> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, true);

    return this.http.post<void>(`${this.baseUrl}/monobank`, request, { context }).pipe(switchMap(() => this.load()));
  }

  update(id: number, request: UpdateBankConnectionRequest): Observable<BankConnection[]> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, true);

    return this.http.put<void>(`${this.baseUrl}/${id}`, request, { context }).pipe(switchMap(() => this.load()));
  }

  disconnect(id: number): Observable<void> {
    return this.http
      .delete<void>(`${this.baseUrl}/${id}`)
      .pipe(tap(() => this.state$.next(this.state$.value.filter(connection => connection.id !== id))));
  }

  importHistory(request: BankHistoryImportRequest): Observable<BankHistoryImportResponse> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, true);

    return this.http.post<BankHistoryImportResponse>(`${this.baseUrl}/history`, request, { context });
  }

  syncNow(): Observable<number> {
    return this.http.post<number>(`${this.baseUrl}/sync`, null);
  }
}
