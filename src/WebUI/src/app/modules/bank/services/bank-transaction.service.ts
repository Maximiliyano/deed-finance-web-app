import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BankTransactionsPage, BankTransactionType } from '../models/bank.model';

@Injectable({ providedIn: 'root' })
export class BankTransactionService {
  private readonly http = inject(HttpClient);

  loadPage(page: number, pageSize: number, type: BankTransactionType | null): Observable<BankTransactionsPage> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);

    if (type) {
      params = params.set('type', type);
    }

    return this.http.get<BankTransactionsPage>('/api/bank-transactions', { params });
  }
}
