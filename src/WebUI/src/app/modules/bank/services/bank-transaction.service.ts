import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SKIP_ERROR_TOAST } from '../../../core/interceptors/error.interceptor';
import {
  BankImportStatus,
  BankProvider,
  BankSummary,
  BankSummaryPeriod,
  BankTransactionsPage,
  BankTransactionType
} from '../models/bank.model';

@Injectable({ providedIn: 'root' })
export class BankTransactionService {
  private readonly http = inject(HttpClient);

  loadPage(
    page: number,
    pageSize: number,
    type: BankTransactionType | null,
    status: BankImportStatus | null = null,
    connectionId: number | null = null,
    provider: BankProvider | null = null,
    currency: string | null = null
  ): Observable<BankTransactionsPage> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);

    if (type) {
      params = params.set('type', type);
    }

    if (status) {
      params = params.set('status', status);
    }

    if (connectionId !== null) {
      params = params.set('connectionId', connectionId);
    }

    if (provider) {
      params = params.set('provider', provider);
    }

    if (currency) {
      params = params.set('currency', currency);
    }

    return this.http.get<BankTransactionsPage>('/api/bank-transactions', { params });
  }

  summary(
    period: BankSummaryPeriod,
    connectionId: number | null,
    provider: BankProvider | null = null
  ): Observable<BankSummary> {
    let params = new HttpParams().set('period', period);

    if (connectionId !== null) {
      params = params.set('connectionId', connectionId);
    }

    if (provider) {
      params = params.set('provider', provider);
    }

    return this.http.get<BankSummary>('/api/bank-transactions/summary', { params });
  }

  addToCapital(id: number, categoryId: number | null): Observable<void> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, true);

    return this.http.post<void>(`/api/bank-notifications/${id}/import`, { categoryId }, { context });
  }

  dismiss(id: number): Observable<void> {
    return this.http.post<void>(`/api/bank-notifications/${id}/dismiss`, null);
  }
}
