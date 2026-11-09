import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateUtilityBillRequest,
  PayUtilityBillRequest,
  UpdateUtilityBillRequest,
  UtilityBillResponse
} from '../models/utility-bill';

@Injectable({ providedIn: 'root' })
export class UtilityBillService {
  private readonly http = inject(HttpClient);

  private readonly baseUrl = '/api/utility-bills';

  getAll(): Observable<UtilityBillResponse[]> {
    return this.http.get<UtilityBillResponse[]>(this.baseUrl);
  }

  create(request: CreateUtilityBillRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request);
  }

  update(id: number, request: UpdateUtilityBillRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  pay(id: number, request: PayUtilityBillRequest): Observable<number> {
    return this.http.post<number>(`${this.baseUrl}/${id}/pay`, request);
  }
}
