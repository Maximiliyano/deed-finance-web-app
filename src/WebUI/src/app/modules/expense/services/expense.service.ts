import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CreateExpenseRequest } from '../models/create-expense-request';
import { ExpenseCategoryResponse } from '../models/expense-category-response';
import { UpdateExpenseRequest } from '../models/update-expense.request';

@Injectable({
  providedIn: 'root'
})
export class ExpenseService {
  private baseUrl = environment.apiUrl + "/api/expenses";

  constructor(private readonly http: HttpClient) { }

  getAllByCategories(capitalId?: number): Observable<ExpenseCategoryResponse[]> {
    let params = new HttpParams();

    if (capitalId != null) {
      params = params.set("capitalId", capitalId);
    }

    return this.http.get<ExpenseCategoryResponse[]>(this.baseUrl, { params,withCredentials: true });
  }

  create(request: CreateExpenseRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request, { withCredentials: true });
  }

  update(request: UpdateExpenseRequest): Observable<void> {
    return this.http.put<void>(this.baseUrl, request, { withCredentials: true });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { withCredentials: true });
  }
}