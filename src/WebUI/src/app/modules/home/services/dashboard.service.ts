import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { DashboardSummary, DashboardSummaryParams } from '../models/dashboard-summary.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/dashboard/summary';

  load(params: DashboardSummaryParams = {}): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(this.baseUrl, { params: this.toHttpParams(params) });
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
