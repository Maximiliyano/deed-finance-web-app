import { HttpParams } from '@angular/common/http';

export interface QueryTimeRange {
  periodStart: Date;
  periodEnd: Date;
}

export function withTimeRange(params: HttpParams, range: QueryTimeRange | null): HttpParams {
  if (!range) return params;

  return params.set('periodStart', range.periodStart.toISOString()).set('periodEnd', range.periodEnd.toISOString());
}
