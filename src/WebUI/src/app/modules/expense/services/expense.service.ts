import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, tap, throwError } from 'rxjs';
import { CreateExpenseRequest } from '../models/create-expense-request';
import { ExpenseCategoryResponse } from '../models/expense-category-response';
import { ExpenseResponse } from '../models/expense-response';
import { UpdateExpenseRequest } from '../models/update-expense.request';

@Injectable({
  providedIn: 'root'
})
export class ExpenseService {
  private readonly http = inject(HttpClient);

  private readonly baseUrl = '/api/expenses';

  private readonly state$ = new BehaviorSubject<ExpenseCategoryResponse[]>([]);
  readonly expenses$ = this.state$.asObservable();

  get current(): ExpenseCategoryResponse[] { return this.state$.value; }

  load(capitalId?: number): Observable<ExpenseCategoryResponse[]> {
    let params = new HttpParams();
    if (capitalId != null) params = params.set("capitalId", capitalId);
    return this.http.get<ExpenseCategoryResponse[]>(this.baseUrl, { params })
      .pipe(tap(items => this.state$.next(items)));
  }

  refresh(capitalId?: number): void { this.load(capitalId).subscribe(); }

  create(request: CreateExpenseRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request).pipe(
      tap(() => this.refresh())
    );
  }

  update(request: UpdateExpenseRequest): Observable<void> {
    return this.http.put<void>(this.baseUrl, request).pipe(
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
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      tap(() => this.refresh()),
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }
}
