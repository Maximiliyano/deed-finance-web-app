import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, tap, throwError } from 'rxjs';
import { BudgetEstimation } from '../models/budget-estimation';
import { QueryTimeRange } from '../models/query-time-range';
import { CapitalResponse } from '../../capital/models/capital-response';
import { CreateBudgetEstimationRequest } from '../models/budget-estimation-request-create';
import { UpdateBudgetEstimationRequest } from '../models/budget-estimation-request-update';

@Injectable({ providedIn: 'root' })
export class BudgetEstimationService {
  private readonly http = inject(HttpClient);

  private readonly baseUrl = '/api/budget-estimations';

  private readonly state$ = new BehaviorSubject<BudgetEstimation[]>([]);
  readonly estimations$ = this.state$.asObservable();

  get current(): BudgetEstimation[] { return this.state$.value; }

  load(query: QueryTimeRange): Observable<BudgetEstimation[]> {
    return this.http.post<BudgetEstimation[]>(`${this.baseUrl}/all`, query)
      .pipe(tap(items => this.state$.next(items)));
  }

  create(request: CreateBudgetEstimationRequest, capital?: CapitalResponse | null): Observable<number> {
    const tempId = -Date.now();
    const previous = this.state$.value;
    const optimistic: BudgetEstimation = {
      id: tempId,
      description: request.description,
      budgetAmount: request.budgetAmount,
      budgetCurrency: request.budgetCurrency,
      capitalId: capital?.id ?? null,
      capitalName: capital?.name ?? null,
      capitalBalance: capital?.balance ?? 0,
      capitalCurrency: capital?.currency ?? null,
      orderIndex: previous.length,
      isCompleted: false
    };

    this.state$.next([...previous, optimistic]);

    return this.http.post<number>(this.baseUrl, request).pipe(
      tap(realId => {
        this.state$.next(this.state$.value.map(e => e.id === tempId ? { ...e, id: realId } : e));
      }),
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  update(id: number, request: UpdateBudgetEstimationRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request).pipe(
      tap(() => {
        this.state$.next(this.state$.value.map(e => e.id === id
          ? { ...e, description: request.description, budgetAmount: request.budgetAmount, budgetCurrency: request.budgetCurrency.toString(), capitalId: request.capitalId, isCompleted: request.isCompleted } as BudgetEstimation
          : e));
      }),
      catchError(err => {
        return throwError(() => err);
      })
    );
  }

  delete(id: number): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.filter(e => e.id !== id));
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  updateOrder(estimations: { id: number; orderIndex: number }[]): Observable<void> {
    const previous = this.state$.value;
    const orderMap = new Map(estimations.map(o => [o.id, o.orderIndex]));
    this.state$.next(
      [...previous].sort((a, b) =>
        (orderMap.get(a.id) ?? a.orderIndex) - (orderMap.get(b.id) ?? b.orderIndex)
      )
    );
    return this.http.put<void>(`${this.baseUrl}/orders`, { estimations }).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }
}
