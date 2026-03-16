import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { AddCapitalRequest } from '../models/add-capital-request';
import { UpdateCapitalRequest } from '../models/update-capital-request';
import { CapitalResponse } from '../models/capital-response';
import { UpdateCapitalOrderRequest } from '../models/update-capital-order-request';
import { QueryParams } from '../../../core/models/query-params';
import { CurrencyType } from '../../../core/types/currency-type';

@Injectable({
  providedIn: 'root'
})
export class CapitalService {
  private baseApiUrl = environment.apiUrl + '/api/capitals';

  constructor(private readonly httpClient: HttpClient) { }

  getAll(params: QueryParams): Observable<CapitalResponse[]> {
      return this.httpClient.post<CapitalResponse[]>(`${this.baseApiUrl}/all`, params, {withCredentials: true});
  }

  getById(id: number): Observable<CapitalResponse> {
    return this.httpClient.get<CapitalResponse>(`${this.baseApiUrl}/${id}`, { withCredentials: true });
  }

  create(request: AddCapitalRequest): Observable<number> {
    return this.httpClient.post<number>(this.baseApiUrl, request, { withCredentials: true });
  }

  update(id: number, request: UpdateCapitalRequest): Observable<void> {
    return this.httpClient.put<void>(`${this.baseApiUrl}/${id}`, request, { withCredentials: true });
  }

  updateOrder(request: UpdateCapitalOrderRequest): Observable<void> {
    return this.httpClient.put<void>(`${this.baseApiUrl}/orders`, request, { withCredentials: true });
  }

  patchSavingsOnly(id: number, value: boolean): Observable<void> {
    return this.httpClient.patch<void>(`${this.baseApiUrl}/${id}/savings-only`, value, { withCredentials: true });
  }

  patchIncludeTotal(id: number, value: boolean): Observable<void> {
    return this.httpClient.patch<void>(`${this.baseApiUrl}/${id}/include-in-total`, value, { withCredentials: true });
  }

  delete(id: number): Observable<void> {
    return this.httpClient.delete<void>(`${this.baseApiUrl}/${id}`, { withCredentials: true });
  }

  getMainCurrency(): { str: string; val: CurrencyType } {
    const val = CurrencyType.UAH;
    return { str: CurrencyType[val], val };
  }
}
