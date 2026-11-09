import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, tap, throwError } from 'rxjs';
import { IncomeResponse } from '../models/income-response';
import { CategoryResponse } from '../../category/models/category-model';
import { CreateIncomeRequest } from '../models/create-income-request';

@Injectable({
  providedIn: 'root'
})
export class IncomeService {
  private readonly http = inject(HttpClient);

  private readonly baseUrl = '/api/incomes';

  private readonly state$ = new BehaviorSubject<IncomeResponse[]>([]);
  private readonly categoriesState$ = new BehaviorSubject<CategoryResponse[]>([]);

  readonly incomes$ = this.state$.asObservable();
  readonly categories$ = this.categoriesState$.asObservable();

  get current(): IncomeResponse[] { return this.state$.value; }

  load(): Observable<IncomeResponse[]> {
    return this.http.get<IncomeResponse[]>(this.baseUrl).pipe(
      tap(data => {
        this.state$.next(data);
      })
    );
  }

  refresh(): void { this.load().subscribe(); }

  create(request: CreateIncomeRequest): Observable<number> {
    const tempId = -Date.now();
    const previous = this.state$.value;
    const optimistic: IncomeResponse = {
      id: tempId,
      capitalId: request.capitalId,
      categoryId: request.categoryId,
      amount: request.amount,
      paymentDate: request.paymentDate,
      purpose: request.purpose
    };
    this.state$.next([optimistic, ...previous]);

    return this.http.post<number>(this.baseUrl, request).pipe(
      tap(realId => {
        this.state$.next(this.state$.value.map(i => i.id === tempId ? { ...i, id: realId } : i));
        this.refresh();
      }),
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  update(request: { id: number; categoryId?: number; amount?: number; purpose: string | null; paymentDate?: string }): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.map(i => i.id === request.id
      ? { ...i,
          categoryId: request.categoryId ?? i.categoryId,
          amount: request.amount ?? i.amount,
          purpose: request.purpose !== undefined ? request.purpose : i.purpose,
          paymentDate: request.paymentDate ? new Date(request.paymentDate) : i.paymentDate }
      : i));
    return this.http.put<void>(this.baseUrl, request).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  delete(id: number): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.filter(i => i.id !== id));
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }
}
