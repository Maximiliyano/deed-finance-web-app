import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SKIP_ERROR_TOAST } from '../../../core/interceptors/error.interceptor';
import { BankImportStatus, BankTransactionsPage, BankTransactionType } from '../models/bank.model';

@Injectable({ providedIn: 'root' })
export class BankTransactionService {
  private readonly http = inject(HttpClient);

  loadPage(
    page: number,
    pageSize: number,
    type: BankTransactionType | null,
    status: BankImportStatus | null = null
  ): Observable<BankTransactionsPage> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);

    if (type) {
      params = params.set('type', type);
    }

    if (status) {
      params = params.set('status', status);
    }

    return this.http.get<BankTransactionsPage>('/api/bank-transactions', { params });
  }

  addToCapital(id: number, categoryId: number | null): Observable<void> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, true);

    return this.http.post<void>(`/api/bank-notifications/${id}/import`, { categoryId }, { context });
  }

  dismiss(id: number): Observable<void> {
    return this.http.post<void>(`/api/bank-notifications/${id}/dismiss`, null);
  }
}
