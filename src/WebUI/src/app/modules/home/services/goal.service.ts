import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
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

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<Goal[]> {
    return this.http.get<Goal[]>(this.baseUrl, { withCredentials: true });
  }

  create(request: CreateGoalRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request, { withCredentials: true });
  }

  update(id: number, request: UpdateGoalRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request, { withCredentials: true });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { withCredentials: true });
  }

  updateOrder(goals: { id: number; orderIndex: number }[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/orders`, { goals }, { withCredentials: true });
  }
}
