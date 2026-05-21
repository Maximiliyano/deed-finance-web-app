import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, tap, throwError } from 'rxjs';
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
  isCompleted: boolean;
}

@Injectable({ providedIn: 'root' })
export class BudgetEstimationService {
  private readonly baseUrl = `${environment.apiUrl}/api/budget-estimations`;

  private readonly state$ = new BehaviorSubject<BudgetEstimation[]>([]);
  readonly estimations$ = this.state$.asObservable();

  constructor(private readonly http: HttpClient) {}

  get current(): BudgetEstimation[] { return this.state$.value; }

  load(): Observable<BudgetEstimation[]> {
    return this.http.get<BudgetEstimation[]>(this.baseUrl, { withCredentials: true })
      .pipe(tap(items => this.state$.next(items)));
  }

  refresh(): void { this.load().subscribe(); }

  getAll(): Observable<BudgetEstimation[]> { return this.load(); }

  create(request: CreateBudgetEstimationRequest): Observable<number> {
    const tempId = -Date.now();
    const previous = this.state$.value;
    const optimistic = {
      id: tempId,
      description: request.description,
      budgetAmount: request.budgetAmount,
      budgetCurrency: String(request.budgetCurrency),
      capitalId: request.capitalId,
      capitalName: null,
      capitalBalance: 0,
      capitalTotalExpense: 0,
      capitalCurrency: null,
      orderIndex: previous.length,
      isCompleted: false
    } as unknown as BudgetEstimation;

    this.state$.next([...previous, optimistic]);

    return this.http.post<number>(this.baseUrl, request, { withCredentials: true }).pipe(
      tap(realId => {
        this.state$.next(this.state$.value.map(e => e.id === tempId ? { ...e, id: realId } : e));
        this.refresh();
      }),
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  update(id: number, request: UpdateBudgetEstimationRequest): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.map(e => e.id === id
      ? { ...e, description: request.description, budgetAmount: request.budgetAmount, budgetCurrency: String(request.budgetCurrency), capitalId: request.capitalId, isCompleted: request.isCompleted } as BudgetEstimation
      : e));
    return this.http.put<void>(`${this.baseUrl}/${id}`, request, { withCredentials: true }).pipe(
      tap(() => this.refresh()),
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  delete(id: number): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.filter(e => e.id !== id));
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { withCredentials: true }).pipe(
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
    return this.http.put<void>(`${this.baseUrl}/orders`, { estimations }, { withCredentials: true }).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }
}
