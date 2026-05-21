import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, tap, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Goal } from '../models/goal.model';

export interface CreateGoalRequest {
  title: string;
  targetAmount: number;
  currency: number;
  currentAmount: number;
  deadline: string | null;
  note: string | null;
}

export interface UpdateGoalRequest {
  title: string;
  targetAmount: number;
  currency: number;
  currentAmount: number;
  deadline: string | null;
  note: string | null;
  isCompleted: boolean;
}

@Injectable({ providedIn: 'root' })
export class GoalService {
  private readonly baseUrl = `${environment.apiUrl}/api/goals`;

  private readonly state$ = new BehaviorSubject<Goal[]>([]);
  readonly goals$ = this.state$.asObservable();

  constructor(private readonly http: HttpClient) {}

  get current(): Goal[] { return this.state$.value; }

  load(): Observable<Goal[]> {
    return this.http.get<Goal[]>(this.baseUrl, { withCredentials: true })
      .pipe(tap(items => this.state$.next(items)));
  }

  refresh(): void { this.load().subscribe(); }

  getAll(): Observable<Goal[]> { return this.load(); }

  create(request: CreateGoalRequest): Observable<number> {
    const tempId = -Date.now();
    const previous = this.state$.value;
    const optimistic: Goal = {
      id: tempId,
      title: request.title,
      targetAmount: request.targetAmount,
      currency: String(request.currency),
      currentAmount: request.currentAmount,
      deadline: request.deadline,
      note: request.note,
      isCompleted: false,
      orderIndex: previous.length
    };
    this.state$.next([...previous, optimistic]);

    return this.http.post<number>(this.baseUrl, request, { withCredentials: true }).pipe(
      tap(realId => {
        this.state$.next(this.state$.value.map(g => g.id === tempId ? { ...g, id: realId } : g));
        this.refresh();
      }),
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  update(id: number, request: UpdateGoalRequest): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.map(g => g.id === id
      ? { ...g, ...request, currency: String(request.currency) }
      : g));
    return this.http.put<void>(`${this.baseUrl}/${id}`, request, { withCredentials: true }).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  delete(id: number): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.filter(g => g.id !== id));
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { withCredentials: true }).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }

  updateOrder(goals: { id: number; orderIndex: number }[]): Observable<void> {
    const previous = this.state$.value;
    const orderMap = new Map(goals.map(o => [o.id, o.orderIndex]));
    this.state$.next(
      [...previous].sort((a, b) =>
        (orderMap.get(a.id) ?? a.orderIndex) - (orderMap.get(b.id) ?? b.orderIndex)
      )
    );
    return this.http.put<void>(`${this.baseUrl}/orders`, { goals }, { withCredentials: true }).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }
}
