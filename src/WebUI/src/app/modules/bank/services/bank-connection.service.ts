import { HttpClient, HttpContext } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, map, switchMap, tap } from 'rxjs';
import { SKIP_ERROR_TOAST } from '../../../core/interceptors/error.interceptor';
import {
  BankConnection,
  BankProvider,
  BankHistoryImportRequest,
  BankHistoryImportResponse,
  ConnectMonobankRequest,
  ConnectPrivatMailRequest,
  ConnectPrivatRequest,
  MonobankAccount,
  PrivatAccount,
  PrivatMailConnection,
  StatementMailInfo,
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

  mailInfo(): Observable<StatementMailInfo> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, true);

    return this.http.get<StatementMailInfo>(`${this.baseUrl}/mail-info`, { context });
  }

  connectPrivatMail(request: ConnectPrivatMailRequest): Observable<PrivatMailConnection> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, true);

    return this.http
      .post<PrivatMailConnection>(`${this.baseUrl}/privat-mail`, request, { context })
      .pipe(switchMap(connection => this.load().pipe(map(() => connection))));
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

  setExcluded(id: number, excluded: boolean): Observable<BankConnection[]> {
    return this.http.put<void>(`${this.baseUrl}/${id}/excluded`, { excluded }).pipe(switchMap(() => this.load()));
  }

  disconnectProvider(provider: BankProvider): Observable<BankConnection[]> {
    return this.http.delete<void>(`${this.baseUrl}/provider/${provider}`).pipe(switchMap(() => this.load()));
  }

  importHistory(request: BankHistoryImportRequest): Observable<BankHistoryImportResponse> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, true);

    return this.http.post<BankHistoryImportResponse>(`${this.baseUrl}/history`, request, { context });
  }

  cancelHistory(): Observable<BankConnection[]> {
    return this.http.delete<void>(`${this.baseUrl}/history`).pipe(switchMap(() => this.load()));
  }

  syncNow(): Observable<number> {
    return this.http.post<number>(`${this.baseUrl}/sync`, null);
  }
}
