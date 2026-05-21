import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CreateUtilityBillRequest,
  PayUtilityBillRequest,
  UpdateUtilityBillRequest,
  UtilityBillResponse
} from '../models/utility-bill';

@Injectable({ providedIn: 'root' })
export class UtilityBillService {
  private readonly baseUrl = environment.apiUrl + '/api/utility-bills';

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<UtilityBillResponse[]> {
    return this.http.get<UtilityBillResponse[]>(this.baseUrl, { withCredentials: true });
  }

  create(request: CreateUtilityBillRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request, { withCredentials: true });
  }

  update(id: number, request: UpdateUtilityBillRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request, { withCredentials: true });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { withCredentials: true });
  }

  pay(id: number, request: PayUtilityBillRequest): Observable<number> {
    return this.http.post<number>(`${this.baseUrl}/${id}/pay`, request, { withCredentials: true });
  }
}
