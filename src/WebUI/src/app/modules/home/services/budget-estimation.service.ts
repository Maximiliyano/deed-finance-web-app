import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { BudgetEstimation } from '../models/budget-estimation.model';

export interface CreateBudgetEstimationRequest {
  description: string;
  budgetAmount: number;
  budgetCurrency: number;
  capitalId: number | null;
}

export interface UpdateBudgetEstimationRequest {
  description: string;
  budgetAmount: number;
  budgetCurrency: number;
  capitalId: number | null;
}

@Injectable({ providedIn: 'root' })
export class BudgetEstimationService {
  private readonly baseUrl = `${environment.apiUrl}/api/budget-estimations`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<BudgetEstimation[]> {
    return this.http.get<BudgetEstimation[]>(this.baseUrl, { withCredentials: true });
  }

  create(request: CreateBudgetEstimationRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request, { withCredentials: true });
  }

  update(id: number, request: UpdateBudgetEstimationRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request, { withCredentials: true });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { withCredentials: true });
  }
}
