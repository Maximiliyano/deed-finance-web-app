import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { DashboardSummary, DashboardSummaryParams } from '../models/dashboard-summary.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/dashboard/summary';

  private readonly state$ = new BehaviorSubject<DashboardSummary | null>(null);
  readonly summary$ = this.state$.asObservable();

  get current(): DashboardSummary | null { return this.state$.value; }

  load(params: DashboardSummaryParams = {}): Observable<DashboardSummary> {
    const isFiltered = !!params.capitalIds?.length;

    return this.http.get<DashboardSummary>(this.baseUrl, { params: this.toHttpParams(params) })
      .pipe(tap(summary => {
        if (!isFiltered) {
          this.state$.next(summary);
        }
      }));
  }

  private toHttpParams(params: DashboardSummaryParams): HttpParams {
    let httpParams = new HttpParams();

    if (params.currency) httpParams = httpParams.set('currency', params.currency);
    if (params.periodStart) httpParams = httpParams.set('periodStart', params.periodStart.toISOString());
    if (params.periodEnd) httpParams = httpParams.set('periodEnd', params.periodEnd.toISOString());
    for (const id of params.capitalIds ?? []) {
      httpParams = httpParams.append('capitalIds', id);
    }

    return httpParams;
  }
}
