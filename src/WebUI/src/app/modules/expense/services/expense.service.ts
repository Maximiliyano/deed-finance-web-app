import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, catchError, tap, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CreateExpenseRequest } from '../models/create-expense-request';
import { ExpenseCategoryResponse } from '../models/expense-category-response';
import { ExpenseResponse } from '../models/expense-response';
import { UpdateExpenseRequest } from '../models/update-expense.request';

@Injectable({
  providedIn: 'root'
})
export class ExpenseService {
  private baseUrl = environment.apiUrl + "/api/expenses";

  private readonly state$ = new BehaviorSubject<ExpenseCategoryResponse[]>([]);
  readonly expenses$ = this.state$.asObservable();

  constructor(private readonly http: HttpClient) { }

  get objects(): ExpenseCategoryResponse[] { return this.state$.value; }

  load(capitalId?: number): Observable<ExpenseCategoryResponse[]> {
    let params = new HttpParams();
    if (capitalId != null) params = params.set("capitalId", capitalId);
    return this.http.get<ExpenseCategoryResponse[]>(this.baseUrl, { params, withCredentials: true })
      .pipe(tap(items => this.state$.next(items)));
  }

  refresh(capitalId?: number): void { this.load(capitalId).subscribe(); }

  getAllByCategories(capitalId?: number): Observable<ExpenseCategoryResponse[]> {
    return this.load(capitalId);
  }

  create(request: CreateExpenseRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request, { withCredentials: true }).pipe(
      tap(() => this.refresh())
    );
  }

  update(request: UpdateExpenseRequest): Observable<void> {
    // Update may move an expense across categories, change date, etc. — refresh after success.
    return this.http.put<void>(this.baseUrl, request, { withCredentials: true }).pipe(
      tap(() => this.refresh())
    );
  }

  delete(id: number): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous
      .map(cat => {
        const expenses = cat.expenses.filter((e: ExpenseResponse) => e.id !== id);
        const categorySum = expenses.reduce((s, e) => s + e.amount, 0);
        return { ...cat, expenses, categorySum };
      })
      .filter(cat => cat.expenses.length > 0));
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { withCredentials: true }).pipe(
      tap(() => this.refresh()),
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }
}
