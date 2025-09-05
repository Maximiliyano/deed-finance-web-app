import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment.development';
import { ExpenseResponse } from '../models/expense-response';
import { CreateExpenseRequest } from '../models/create-expense-request';
import { ExpenseCategoryResponse } from '../models/expense-category-response';

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

    return this.http.get<ExpenseCategoryResponse[]>(this.baseUrl, { params });
  }

  create(request: CreateExpenseRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request);
  }
}
