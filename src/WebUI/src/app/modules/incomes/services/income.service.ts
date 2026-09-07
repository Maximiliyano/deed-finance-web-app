import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { BehaviorSubject, Observable, catchError, tap, throwError } from 'rxjs';
import { IncomeResponse } from '../models/income-response';
import { CategoryResponse } from '../../category/models/category-model';
import { CreateIncomeRequest } from '../models/create-income-request';

@Injectable({
  providedIn: 'root'
})
export class IncomeService {
  private baseUrl = environment.apiUrl + "/api/incomes";

  private readonly incomesState$ = new BehaviorSubject<IncomeResponse[]>([]);
  private readonly categoriesState$ = new BehaviorSubject<CategoryResponse[]>([]);

  readonly incomes$ = this.incomesState$.asObservable();
  readonly categories$ = this.categoriesState$.asObservable();

  constructor(private readonly http: HttpClient) { }

  get currentIncomes(): IncomeResponse[] { return this.incomesState$.value; }

  load(): Observable<IncomeResponse[]> {
    return this.http.get<IncomeResponse[]>(this.baseUrl, { withCredentials: true }).pipe(
      tap(data => {
        this.incomesState$.next(data);
      })
    );
  }

  refresh(): void { this.load().subscribe(); }

  getAll(): Observable<IncomeResponse[]> { return this.load(); }

  create(request: CreateIncomeRequest): Observable<number> {
    const tempId = -Date.now();
    const previous = this.incomesState$.value;
    const optimistic: IncomeResponse = {
      id: tempId,
      capitalId: request.capitalId,
      categoryId: request.categoryId,
      amount: request.amount,
      paymentDate: request.paymentDate,
      purpose: request.purpose
    };
    this.incomesState$.next([optimistic, ...previous]);

    return this.http.post<number>(this.baseUrl, request, { withCredentials: true }).pipe(
      tap(realId => {
        this.incomesState$.next(this.incomesState$.value.map(i => i.id === tempId ? { ...i, id: realId } : i));
        this.refresh();
      }),
      catchError(err => {
        this.incomesState$.next(previous);
        return throwError(() => err);
      })
    );
  }

  update(request: { id: number; categoryId?: number; amount?: number; purpose?: string; paymentDate?: string }): Observable<void> {
    const previous = this.incomesState$.value;
    this.incomesState$.next(previous.map(i => i.id === request.id
      ? { ...i,
          categoryId: request.categoryId ?? i.categoryId,
          amount: request.amount ?? i.amount,
          purpose: request.purpose !== undefined ? request.purpose : i.purpose,
          paymentDate: request.paymentDate ? new Date(request.paymentDate) : i.paymentDate }
      : i));
    return this.http.put<void>(this.baseUrl, request, { withCredentials: true }).pipe(
      catchError(err => {
        this.incomesState$.next(previous);
        return throwError(() => err);
      })
    );
  }

  delete(id: number): Observable<void> {
    const previous = this.incomesState$.value;
    this.incomesState$.next(previous.filter(i => i.id !== id));
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { withCredentials: true }).pipe(
      catchError(err => {
        this.incomesState$.next(previous);
        return throwError(() => err);
      })
    );
  }
}
