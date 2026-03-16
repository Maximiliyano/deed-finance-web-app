import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Debt } from '../models/debt.model';

export interface CreateDebtRequest {
  item: string;
  amount: number;
  currency: number;
  source: string;
  recipient: string;
  borrowedAt: string;
  deadlineAt: string | null;
  note: string | null;
  capitalId: number | null;
}

export interface UpdateDebtRequest {
  item: string;
  amount: number;
  currency: number;
  source: string;
  recipient: string;
  borrowedAt: string;
  deadlineAt: string | null;
  note: string | null;
  isPaid: boolean;
  payFromCapitalId: number | null;
}

@Injectable({ providedIn: 'root' })
export class DebtService {
  private readonly baseUrl = `${environment.apiUrl}/api/debts`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<Debt[]> {
    return this.http.get<Debt[]>(this.baseUrl, { withCredentials: true });
  }

  create(request: CreateDebtRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request, { withCredentials: true });
  }

  update(id: number, request: UpdateDebtRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request, { withCredentials: true });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { withCredentials: true });
  }
}
